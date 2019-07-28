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

namespace ArkWebMapAnalytics
{
    class Program
    {
        public static Random rand;
        public static LiteDatabase db;

        static void Main(string[] args)
        {
            rand = new Random();
            if (File.Exists("log.db"))
                File.Copy("log.db", "log.db.bak_" + DateTime.UtcNow.Ticks.ToString());
            db = new LiteDatabase("log.db");

            MainAsync().GetAwaiter().GetResult();
        }

        public static Task MainAsync()
        {
            var host = new WebHostBuilder()
                .UseKestrel(options =>
                {
                    IPAddress addr = IPAddress.Any;
                    options.Listen(addr, 43294);

                })
                .UseStartup<Program>()
                .Build();

            return host.RunAsync();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.Run(OnHttpRequest);
        }

        public static async Task QuickWriteToDoc(Microsoft.AspNetCore.Http.HttpContext context, string content, string type = "text/html", int code = 200)
        {
            var response = context.Response;
            response.StatusCode = code;
            response.ContentType = type;

            //Load the template.
            string html = content;
            var data = Encoding.UTF8.GetBytes(html);
            response.ContentLength = data.Length;
            await response.Body.WriteAsync(data, 0, data.Length);
        }

        public static T DecodePostBody<T>(Microsoft.AspNetCore.Http.HttpContext context)
        {
            string buffer;
            using (StreamReader sr = new StreamReader(context.Request.Body))
                buffer = sr.ReadToEnd();

            //Deserialize
            return JsonConvert.DeserializeObject<T>(buffer);
        }

        public static async Task QuickWriteJsonToDoc<T>(Microsoft.AspNetCore.Http.HttpContext context, T data, int code = 200)
        {
            await QuickWriteToDoc(context, JsonConvert.SerializeObject(data, Formatting.Indented), "application/json", code);
        }

        public static string GenerateRandomID()
        {
            return GenerateRandomStringCustom(24, "1234567890ABCDEF".ToCharArray());
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

        public static async Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e)
        {
            //Set CORS headers
            e.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST");
            e.Response.Headers.Add("Access-Control-Allow-Origin", "https://deltamap.net");

            //Check if this is a CORS request
            string method = e.Request.Method.ToUpper();
            if(method == "OPTIONS")
            {
                e.Response.Headers.Add("Cache-Control", "max-age=31536000"); //Ensures we do not check again
                await QuickWriteToDoc(e, "CORS OK", "text/plain");
                return;
            }

            //Handle
            try
            {
                if (e.Request.Path.ToString() == "/v1/action" && method == "POST")
                {
                    await Services.ActionsService.OnHttpRequest(e);
                    return;
                }
                if (e.Request.Path.ToString() == "/v1/archive" && method == "GET")
                {
                    await Services.ArchiveService.OnHttpRequest(e);
                    return;
                }

                throw new StandardError("Not Found", 404);
            } catch (StandardError sx)
            {
                await QuickWriteToDoc(e, sx.errorMsg, "text/plain", sx.errorCode);
            } catch (Exception ex)
            {
                await QuickWriteToDoc(e, "Unexpected Server Error", "text/plain", 500);
                Console.WriteLine($"ERROR {ex.Message} @ {ex.StackTrace}");
            }
        }
    }
}
