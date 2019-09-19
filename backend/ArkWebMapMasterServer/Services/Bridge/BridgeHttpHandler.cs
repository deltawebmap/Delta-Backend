using ArkBridgeSharedEntities.Entities;
using ArkWebMapMasterServer.PresistEntities;
using LibDeltaSystem.Db.System;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Bridge
{
    public class BridgeHttpHandler
    {
        public const int MIN_SLAVE_VERSION = 0;
        public const int LATEST_RELEASE_VERSION = 1;
        public const string LATEST_RELEASE_URL = "";
        public const string LATEST_RELEASE_NOTES = "A test for the updater system.\n\nCool!";

        public static Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, string path)
        {
            //Authenticate the slave server. First, find it by it's id. This is passed in with the header.
            string serverId = e.Request.Headers["X-Ark-Slave-Server-ID"];

            //Get the server by ID
            DbServer server = ArkWebMapMasterServer.Servers.ArkSlaveServerSetup.GetSlaveServerById(serverId);

            //If the server ID was invalid, fail
            if (server == null)
                throw new StandardError("Could not find slave server.", StandardErrorCode.SlaveAuthFailed);

            //Validate the integrity
            if(!Program.CompareByteArrays(server.server_creds, Convert.FromBase64String(e.Request.Headers["X-Ark-Slave-Server-Creds"])))
            {
                throw new StandardError("Could not authenticate slave server.", StandardErrorCode.SlaveAuthFailed);
            }

            //Now, continue normally.
            //Check path
            if (path.StartsWith("hello"))
            {
                //Pass hello request
                return HelloRequest.OnHttpRequest(e, server);
            }
            if (path.StartsWith("world_report"))
            {
                //Pass hello request
                return ArkReportRequest.OnHttpRequest(e, server);
            }
            if (path.StartsWith("v2_send_tribe_notification"))
            {
                return V2ServerNotificationRequest.OnHttpRequest(e, server);
            }
            if (path.StartsWith("mass_request_steam_info"))
            {
                return MassRequestSteamDataRequest.OnHttpRequest(e, server);
            }
            if (path.StartsWith("mirror_report"))
            {
                return MirrorReportService.OnHttpRequest(e, server);
            }
            if(path.StartsWith("report_hub"))
            {
                return HubReportRequest.OnHttpRequest(e, server);
            }

            //Not found
            throw new StandardError("Not Found", StandardErrorCode.NotFound);
        }
    }
}
