using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using LibDeltaSystem;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;

namespace ArkWebMapGateway
{
    class Program
    {
        static Random rand;
        public static DeltaConnection conn;
        public static SystemConfigFile config;

        static void Main(string[] args)
        {
            Console.WriteLine("Starting ArkWebMap WebSocket Gateway...");

            //Init
            rand = new Random();
            ClientHolder.connections = new List<GatewayConnection>();

            //Load config
            config = JsonConvert.DeserializeObject<SystemConfigFile>(File.ReadAllText(args[0]));

            //Connect to database
            conn = new DeltaConnection(config.db, "gateway", 0, 0);
            conn.Connect().GetAwaiter().GetResult();

            //Start
            MainAsync().GetAwaiter().GetResult();
        }

        public static Task MainAsync()
        {
            var host = new WebHostBuilder()
                .UseKestrel(options =>
                {
                    IPAddress addr = IPAddress.Any;
                    options.Listen(addr, config.port, listenOptions =>
                    {
                        listenOptions.UseHttps(config.cert_pathname, config.cert_password);
                    });
                })
                .UseStartup<Program>()
                .Build();

            return host.RunAsync();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseWebSockets(new WebSocketOptions
            {
                ReceiveBufferSize = 1024 * 16
            });
            app.Run(GatewayHttpHandler.OnHttpRequest);
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
            //Read post body
            byte[] buffer = new byte[(int)context.Request.ContentLength];
            context.Request.Body.Read(buffer, 0, buffer.Length);

            //Deserialize
            return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(buffer));
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
    }
}
