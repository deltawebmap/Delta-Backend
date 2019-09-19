using ArkBridgeSharedEntities.Entities;
using ArkWebMapMasterServer.NetEntities;
using ArkWebMapMasterServer.PresistEntities;
using ArkWebMapMasterServer.Services.Servers;
using ArkWebMapMasterServer.Tools;
using LibDeltaSystem.Db.System;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Misc
{
    public class ArkSetupProxy
    {
        public static Dictionary<string, ArkSetupProxySession> setupProxySessions = new Dictionary<string, ArkSetupProxySession>();

        public static Task OnObtainCode(Microsoft.AspNetCore.Http.HttpContext e)
        {
            //Generate a unique session ID
            string id = Program.GenerateRandomStringCustom(6, "1234567890".ToCharArray());
            while (setupProxySessions.ContainsKey(id))
                id = Program.GenerateRandomStringCustom(6, "1234567890".ToCharArray());

            //Create session
            setupProxySessions.Add(id, new ArkSetupProxySession
            {
                toServer = new List<ArkBridgeSharedEntities.Entities.ArkSetupProxyMessage>(),
                toWeb = new List<ArkBridgeSharedEntities.Entities.ArkSetupProxyMessage>(),
                up = false
            });

            return Program.QuickWriteJsonToDoc(e, new SetupServerProxy_ObtainCode
            {
                code = id
            });
        }

        public static Task OnCreateProxySessionRequest(Microsoft.AspNetCore.Http.HttpContext e, DbUser user)
        {
            //Get the session.
            string sessionId = e.Request.Query["session_id"];
            if (!setupProxySessions.ContainsKey(sessionId))
            {
                //Failed
                return Program.QuickWriteJsonToDoc(e, new ServerSetupWizard_BeginReply
                {
                    display_id = sessionId,
                    ok = false
                });
            }

            //Stop if already claimed
            if(setupProxySessions[sessionId].claimed)
            {
                return Program.QuickWriteJsonToDoc(e, new ServerSetupWizard_BeginReply
                {
                    display_id = sessionId,
                    ok = false
                });
            }

            //Create server
            DbServer server = CreateServerFromPOST(e, user, sessionId);

            //Return 
            return Program.QuickWriteJsonToDoc(e, new ServerSetupWizard_BeginReply
            {
                display_id = sessionId,
                request_url = $"https://deltamap.net/api/server_setup_proxy/{sessionId}?from=WebClient",
                server = server,
                ok = true
            });
        }

        public static Task OnCreateProxySessionHeadlessRequest(Microsoft.AspNetCore.Http.HttpContext e, DbUser user)
        {
            //Create the session
            string sessionId = Program.GenerateRandomStringCustom(32, "QWERTYUIOPASDFGHJKLZXCVBNM1234567890".ToCharArray());
            while (setupProxySessions.ContainsKey(sessionId))
                sessionId = Program.GenerateRandomStringCustom(32, "QWERTYUIOPASDFGHJKLZXCVBNM1234567890".ToCharArray());

            //Create session
            setupProxySessions.Add(sessionId, new ArkSetupProxySession
            {
                toServer = new List<ArkBridgeSharedEntities.Entities.ArkSetupProxyMessage>(),
                toWeb = new List<ArkBridgeSharedEntities.Entities.ArkSetupProxyMessage>(),
                up = false
            });

            //Create server
            DbServer server = CreateServerFromPOST(e, user, sessionId);

            //Create a JSON file to download, then put it up for download
            MemoryStream ms = new MemoryStream();
            byte[] json = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new ServerSetupHeadlessFile
            {
                time = DateTime.UtcNow,
                token = sessionId,
                version = 1
            }));
            ms.Write(json, 0, json.Length);
            string url = TokenFileDownloadTool.PutFile(ms, "headless_setup.json");

            //Return 
            return Program.QuickWriteJsonToDoc(e, new ServerSetupWizard_BeginReplyHeadless
            {
                display_id = sessionId,
                request_url = $"https://deltamap.net/api/server_setup_proxy/{sessionId}?from=WebClient",
                server = server,
                ok = true,
                headless_config_url = url
            });
        }

        private static DbServer CreateServerFromPOST(Microsoft.AspNetCore.Http.HttpContext e, DbUser user, string sessionId)
        {
            //Grab payload for server creation
            EditServerListingPayload payload = Program.DecodePostBody<EditServerListingPayload>(e);

            //Create server
            DbServer server = ArkWebMapMasterServer.Servers.ArkSlaveServerSetup.CreateServer("Setup Server", null, user);

            //Edit
            EditServerListing.EditServer(server, payload, user);
            server.Update();

            //Create session
            setupProxySessions[sessionId].server = server;
            setupProxySessions[sessionId].user = user;
            setupProxySessions[sessionId].up = true;
            setupProxySessions[sessionId].claimed = true;

            return server;
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
