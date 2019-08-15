using System;
using System.IO;
using System.Threading.Tasks;
using LibDelta;
using LiteDB;
using Newtonsoft.Json;
using ArkWebMapOfflineData.Entities;

namespace ArkWebMapOfflineData
{
    class Program
    {
        public static LiteDatabase db;
        public static SystemConfigFile config;

        static void Main(string[] args)
        {
            //Read config file
            config = JsonConvert.DeserializeObject<SystemConfigFile>(File.ReadAllText(args[0]));
            
            //Init DB
            db = new LiteDatabase(config.db_path);

            //Init LibDelta
            DeltaMapTools.Init(config.system_key, "Offline-Data", new ArkWebMapGatewayClient.GatewayMessageHandler());
            
            //Start server
            WebServerTools.StartWebServer(config.port, OnHttpRequest).GetAwaiter().GetResult();
        }

        public static LiteCollection<DataServer> GetServerCollection()
        {
            return db.GetCollection<DataServer>("servers");
        }

        public static LiteCollection<DataCommit> GetCommitCollection()
        {
            return db.GetCollection<DataCommit>("commits");
        }

        public static async Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e)
        {
            //Set CORS headers
            e.Response.Headers.Add("Server", "DeltaWebMap Offline Data");
            e.Response.Headers.Add("Access-Control-Allow-Headers", "Authorization");
            e.Response.Headers.Add("Access-Control-Allow-Origin", "https://deltamap.net");
            e.Response.Headers.Add("Access-Control-Allow-Methods", "GET");

            //Do CORS stuff
            if(e.Request.Method.ToUpper() == "OPTIONS")
            {
                await WebServerTools.QuickWriteToDoc(e, "CORS OK", "text/plain", 200);
                return;
            }

            //Accept uploads
            if(e.Request.Path.ToString() == "/upload")
            {
                await UploadService.OnHttpRequest(e);
                return;
            }

            //Accept downloads
            if(e.Request.Path.ToString().StartsWith("/content/"))
            {
                await DownloadService.OnHttpRequest(e);
                return;
            }

            //Fail.
            await WebServerTools.QuickWriteToDoc(e, "Invalid Endpoint", "text/plain", 404);
        }
    }
}
