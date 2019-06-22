using ArkWebMapMasterServer.MirrorEntities;
using ArkWebMapMasterServer.PresistEntities;
using ArkWebMapMasterServer.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Mirror
{
    public static class MirrorService
    {
        public static Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e)
        {
            //Get the token and get the matching server
            string token = e.Request.Query["token"];

            //Do auth
            ArkMirrorToken auth = MirrorTokenTool.TryMatchToken(token);
            ArkServer server = ArkWebMapMasterServer.Servers.ArkSlaveServerSetup.GetSlaveServerById(auth._id);

            //Read and parse the body
            List<MirroredMessage> msgs = new List<MirroredMessage>();
            using (StreamReader sr = new StreamReader(e.Request.Body))
            {
                MirrorProtocolReader reader = new MirrorProtocolReader(sr);
                while(true)
                {
                    MirroredMessage msg = reader.ReadMessage();
                    if (msg.opcode == MirroredOpcode.EOS)
                        break;
                    else
                        msgs.Add(msg);
                }
            }

            //Send off events to the GATEWAY
            foreach(var m in msgs)
            {
                m.ProcessMsg(server, auth);
            }

            //Return OK, but Ark won't care
            return Program.QuickWriteStatusToDoc(e, true);
        }
    }
}
