using ArkWebMapGatewayClient;
using ArkWebMapMasterServer.PresistEntities;
using LiteDB;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer
{
    class Program
    {
        public static LiteDatabase db;
        public static LiteDatabase map_db;
        public static AWMGatewayClient gateway;
        public static MasterServerConfig config;
        public static Random rand = new Random();
        public static List<string> onlineServers;
        public static Gateway.GatewayHandler gatewayHandler;
        public const string PREFIX_URL = "https://deltamap.net/api";

        static void Main(string[] args)
        {
            Console.WriteLine("Loading config...");
            config = JsonConvert.DeserializeObject<MasterServerConfig>(File.ReadAllText(args[0]));

            Console.WriteLine("Starting Database...");
            db = new LiteDatabase(config.database_pathname);
            map_db = new LiteDatabase(config.map_database_pathname);

            Console.WriteLine("Connecting to GATEWAY...");
            gatewayHandler = new Gateway.GatewayHandler();
            gateway = AWMGatewayClient.CreateClient(GatewayClientType.MasterServer, "master", "", 1, 0, false, gatewayHandler, config.system_server_keys["master"]);

            Console.WriteLine("Fetching online servers from LIGHTSPEED...");
            FetchOnlineServers();

            Console.WriteLine("Starting some other timers...");
            Tools.TokenFileDownloadTool.Init();

            Console.WriteLine("Starting Kestrel...");
            MainAsync().GetAwaiter().GetResult();
        }

        private static void FetchOnlineServers()
        {
            string text;
            try
            {
                using (WebClient wc = new WebClient())
                    text = wc.DownloadString("https://lightspeed.deltamap.net/online");
                onlineServers = JsonConvert.DeserializeObject<List<string>>(text);
            }
            catch (Exception ex)
            {
                onlineServers = new List<string>();
            }
        }

        public static void Log(string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static Task MainAsync()
        {
            var host = new WebHostBuilder()
                .UseKestrel(options =>
                {
                    IPAddress addr = IPAddress.Any;
                    options.Listen(addr, 43298);

                })
                .UseStartup<Program>()
                .Build();

            return host.RunAsync();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.Run(HttpHandler.OnHttpRequest);

        }

        public static Task QuickWriteToDoc(Microsoft.AspNetCore.Http.HttpContext context, string content, string type = "text/html", int code = 200)
        {
            var response = context.Response;
            response.StatusCode = code;
            response.ContentType = type;

            //Load the template.
            string html = content;
            var data = Encoding.UTF8.GetBytes(html);
            response.ContentLength = data.Length;
            return response.Body.WriteAsync(data, 0, data.Length);
        }

        public static T DecodePostBody<T>(Microsoft.AspNetCore.Http.HttpContext context)
        {
            string buffer;
            using (StreamReader sr = new StreamReader(context.Request.Body))
                buffer = sr.ReadToEnd();

            //Deserialize
            return JsonConvert.DeserializeObject<T>(buffer);
        }

        public static Task QuickWriteStatusToDoc(Microsoft.AspNetCore.Http.HttpContext e, bool ok, int code = 200)
        {
            return QuickWriteJsonToDoc(e, new ArkBridgeSharedEntities.Entities.TrueFalseReply
            {
                ok = ok
            }, code);
        }

        public static Task QuickWriteJsonToDoc<T>(Microsoft.AspNetCore.Http.HttpContext context, T data, int code = 200)
        {
            return QuickWriteToDoc(context, JsonConvert.SerializeObject(data, Formatting.Indented), "application/json", code);
        }

        public static string GenerateRandomString(int length)
        {
            return GenerateRandomStringCustom(length, "qwertyuiopasdfghjklzxcvbnm1234567890QWERTYUIOPASDFGHJKLZXCVBNM".ToCharArray());
        }

        public static string GenerateRandomStringCustom(int length, char[] chars)
        {
            string output = "";
            for (int i = 0; i < length; i++)
            {
                output += chars[rand.Next(0, chars.Length)];
            }
            return output;
        }

        public static byte[] GenerateRandomBytes(int length)
        {
            byte[] buf = new byte[length];
            rand.NextBytes(buf);
            return buf;
        }

        public static RequestHttpMethod FindRequestMethod(Microsoft.AspNetCore.Http.HttpContext context)
        {
            return Enum.Parse<RequestHttpMethod>(context.Request.Method.ToLower());
        }

        public static string GetRequestIP(Microsoft.AspNetCore.Http.HttpContext context)
        {
            //THIS WILL HAVE TO BE CHANGED UNDER YOUR OWN ENVIORNMENT!!!! This relies on my Apache reverse proxy. Please fix the url obtaining.
            return context.Request.Headers["X-Forwarded-For"];
        }

        public static bool CompareByteArrays(byte[] b1, byte[] b2)
        {
            if (b1.Length != b2.Length)
                return false;
            for(int i = 0; i<b1.Length; i++)
            {
                if (b1[i] != b2[i])
                    return false;
            }
            return true;
        }
    }

    public enum RequestHttpMethod
    {
        get,
        post,
        put,
        delete,
        options
    }
}
