using ArkBridgeSharedEntities.Entities;
using ArkWebMapMasterServer.NetEntities;
using ArkWebMapMasterServer.PresistEntities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Misc
{
    public class ArkSetupProxy
    {
        public static Dictionary<string, ArkSetupProxySession> setupProxySessions = new Dictionary<string, ArkSetupProxySession>();

        public static Task OnCreateProxySessionRequest(Microsoft.AspNetCore.Http.HttpContext e, ArkUser user)
        {
            //Generate a unique session ID
            string id = Program.GenerateRandomStringCustom(6, "1234567890".ToCharArray());
            while (setupProxySessions.ContainsKey(id))
                id = Program.GenerateRandomStringCustom(6, "1234567890".ToCharArray());

            //Create server
            ArkServer server = ArkWebMapMasterServer.Servers.ArkSlaveServerSetup.CreateServer("Setup Server", null, user);

            //Temporary, only for demo server creation.
            //server = ArkWebMapMasterServer.Servers.ArkSlaveServerSetup.CreateServer("ArkWebMap Demo", "https://ark.romanport.com/assets/demo_server_icon.png", user, true);

            //Create session
            setupProxySessions.Add(id, new ArkSetupProxySession
            {
                server = server,
                toServer = new List<ArkBridgeSharedEntities.Entities.ArkSetupProxyMessage>(),
                toWeb = new List<ArkBridgeSharedEntities.Entities.ArkSetupProxyMessage>(),
                user = user
            });

            //Return 
            return Program.QuickWriteJsonToDoc(e, new ServerSetupWizard_BeginReply
            {
                display_id = id,
                request_url = $"https://ark.romanport.com/api/server_setup_proxy/{id}?from=WebClient",
                server = server
            });
        }

        public static Task OnSetupProxyHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, string path)
        {
            //Get the session ID.
            if (!setupProxySessions.ContainsKey(path))
                throw new StandardError("Session not found", StandardErrorCode.NotFound);

            //Get requester type
            if (!e.Request.Query.ContainsKey("from"))
                throw new StandardError("Missing 'from' query entry.", StandardErrorCode.MissingRequiredArg);
            if (!Enum.TryParse<ProxySide>(e.Request.Query["from"], out ProxySide from))
                throw new StandardError("Query entry 'from' is invalid.", StandardErrorCode.InvalidInput);

            //Get session
            ArkSetupProxySession session = setupProxySessions[path];

            //Switch depending on type
            RequestHttpMethod method = Program.FindRequestMethod(e);
            if (method == RequestHttpMethod.get)
            {
                //Return results
                if (from == ProxySide.Server)
                {
                    Task t = Program.QuickWriteJsonToDoc(e, session.toServer);
                    session.toServer.Clear();
                    return t;
                }
                if (from == ProxySide.WebClient)
                {
                    Task t = Program.QuickWriteJsonToDoc(e, session.toWeb);
                    session.toWeb.Clear();
                    return t;
                }
                throw new StandardError("Unexpected client.", StandardErrorCode.InvalidInput);
            }
            if (method == RequestHttpMethod.post)
            {
                //Deserialize
                ArkBridgeSharedEntities.Entities.ArkSetupProxyMessage message = Program.DecodePostBody<ArkBridgeSharedEntities.Entities.ArkSetupProxyMessage>(e);

                //Set IP
                message.from_ip = Program.GetRequestIP(e);

                //Write results
                if (from == ProxySide.Server)
                {
                    session.toWeb.Add(message);
                    return Program.QuickWriteJsonToDoc(e, new ArkBridgeSharedEntities.Entities.TrueFalseReply
                    {
                        ok = true
                    });
                }
                if (from == ProxySide.WebClient)
                {
                    session.toServer.Add(message);
                    return Program.QuickWriteJsonToDoc(e, new ArkBridgeSharedEntities.Entities.TrueFalseReply
                    {
                        ok = true
                    });
                }
                throw new StandardError("Unexpected client.", StandardErrorCode.InvalidInput);
            }
            throw new StandardError("Bad method.", StandardErrorCode.NotFound);
        }

        enum ProxySide
        {
            WebClient,
            Server
        }
    }
}
