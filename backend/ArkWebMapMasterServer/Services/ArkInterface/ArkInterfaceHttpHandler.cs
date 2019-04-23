using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.ArkInterface
{
    public class ArkInterfaceHttpHandler
    {
        public static Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, string path)
        {
            //Read basic query args
            if (!e.Request.Query.ContainsKey("version_id") || !e.Request.Query.ContainsKey("api_token"))
                throw new Exception("Missing 'version_id' or 'api_token'.");
            int modVersionId = int.Parse(e.Request.Query["version_id"]);
            string apiToken = e.Request.Query["api_token"];

            //TODO
            Console.WriteLine($"Got ark interface request with version {modVersionId} and token {apiToken}.");
            using (StreamReader sr = new StreamReader(e.Request.Body))
                Console.WriteLine(sr.ReadToEnd());

            //Reply will not be read, but send one anyways
            return Program.QuickWriteToDoc(e, "OK", "text/plain");
        }
    }
}
