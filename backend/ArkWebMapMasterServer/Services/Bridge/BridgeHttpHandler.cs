using ArkWebMapMasterServer.PresistEntities;
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
            string serverValidationString = e.Request.Headers["X-Ark-Slave-Server-Validation"];

            //Get the server by ID
            ArkServer server = ArkWebMapMasterServer.Servers.ArkSlaveServerSetup.GetSlaveServerById(serverId);
            //Worry about validation later...

            //If the server validation failed, stop
            if (server == null)
                throw new StandardError("Could not authenticate slave server.", StandardErrorCode.SlaveAuthFailed);

            //Now, continue normally.
            //Check path
            if (path.StartsWith("hello"))
            {
                //Pass hello request
                return HelloRequest.OnHttpRequest(e, server);
            }

            //Not found
            throw new StandardError("Not Found", StandardErrorCode.NotFound);
        }
    }
}
