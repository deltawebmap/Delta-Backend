using ArkWebMapMasterServer.NetEntities;
using ArkWebMapMasterServer.PresistEntities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Bridge
{
    public static class HelloRequest
    {
        public static Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, ArkServer s)
        {
            //This is a hello request. Set this server to be the most recent server and send back the server info.

            //Open the payload
            SlaveHelloPayload payload = Program.DecodePostBody<SlaveHelloPayload>(e);

            //Check to see if the version is compatible
            if (payload.my_version < BridgeHttpHandler.MIN_SLAVE_VERSION)
                return ReplyWithErrorMessage(e, SlaveHelloReply_MessageType.SlaveOutOfDate, new Dictionary<string, string>
                {
                    {"notes", BridgeHttpHandler.LATEST_RELEASE_NOTES },
                    {"download_url", BridgeHttpHandler.LATEST_RELEASE_URL },
                    {"latest_version", BridgeHttpHandler.LATEST_RELEASE_VERSION.ToString() }
                });

            //Create reply
            SlaveHelloReply reply = new SlaveHelloReply
            {
                serverInfo = s,
                status = SlaveHelloReply_MessageType.Ok,
                status_info = new Dictionary<string, string>()
            };

            //Set as active proxy location
            string slave_url = Program.GetRequestIP(e) + ":" + payload.my_port;
            s.latest_proxy_url = slave_url;
            s.Update();

            //Write reply
            return Program.QuickWriteJsonToDoc(e, reply);
        }

        private static Task ReplyWithErrorMessage(Microsoft.AspNetCore.Http.HttpContext e, SlaveHelloReply_MessageType status, Dictionary<string, string> more)
        {
            //Create reply
            SlaveHelloReply reply = new SlaveHelloReply
            {
                status = status,
                status_info = more
            };
            return Program.QuickWriteJsonToDoc(e, reply);
        }

        
    }
}
