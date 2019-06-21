using ArkWebMapMasterServer.PresistEntities;
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

            //TODO: Do auth
            string serverId = "IRlrzrmxoe7M5rKiKrDD7zCf";
            ArkServer server = ArkWebMapMasterServer.Servers.ArkSlaveServerSetup.GetSlaveServerById(serverId);

            //Read and parse the body
            using (StreamReader sr = new StreamReader(e.Request.Body))
                Console.WriteLine(sr.ReadToEnd());

            //Return OK, but Ark won't care
            return Program.QuickWriteStatusToDoc(e, true);
        }
    }
}
