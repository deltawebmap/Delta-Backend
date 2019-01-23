using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ArkSaveEditor;
using ArkSaveEditor.World;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Threading;
using System.IO;
using System.Collections.Generic;

namespace ArkHttpServer
{
    partial class Program
    {
        public static ArkWorld world;
        public static System.Timers.Timer event_checker_timer;

        public const int SESSION_TIMEOUT_MS = 10000; //Amount of time before a session is timed out and removed. This is the "heartbeat" of the session. It's also how quickly a client will poll the server.

        static void Main(string[] args)
        {
            //Start event checker timer
            event_checker_timer = new System.Timers.Timer(5000);
            event_checker_timer.Elapsed += Event_checker_timer_Elapsed;
            event_checker_timer.Start();

            //Load save editor entries
            ArkImports.ImportContent(@"PrimalData/world.json", @"PrimalData/dinos.json", @"PrimalData/items.json");

            //Start Kestrel
            MainAsync().GetAwaiter().GetResult();
        }

        private static void Event_checker_timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //Check all of our sessions for updated files.

            lock(sessions)
            {
                //First, loop through and find unique files
                Dictionary<string, byte[]> hashes = new Dictionary<string, byte[]>();
                foreach (var s in sessions.Values)
                {
                    if (!hashes.ContainsKey(s.game_file_path))
                        hashes.Add(s.game_file_path, s.GetComputedFileHash());
                }

                //Now, go through each session and compare the hashes
                foreach (var s in sessions.Values)
                {
                    //Get the hash
                    byte[] hash = hashes[s.game_file_path];

                    //Compare
                    bool hasHashChanged = !s.CompareExistingHashWithNewHash(hash);

                    //If the hash has changed, add an event
                    if (hasHashChanged)
                    {
                        s.new_events.Add(new Entities.HttpSessionEvent(null, Entities.HttpSessionEventType.MapUpdate));
                    }

                    //Update the hash
                    s.last_file_hash = hash;
                }

                //Next, loop through clients and find old sessions to remove.
                int max_timeout_time = SESSION_TIMEOUT_MS + 8000;
                List<string> sessionsToRemove = new List<string>();
                foreach (var s in sessions)
                {
                    double timeDiff = (DateTime.UtcNow - s.Value.last_heartbeat_time).TotalMilliseconds;
                    if (timeDiff > max_timeout_time)
                    {
                        //Remove the session.
                        sessionsToRemove.Add(s.Key);
                    }
                }
                foreach (string s in sessionsToRemove)
                    sessions.Remove(s);
            }
        }

        public static ArkWorld GetArkWorld(string pathname)
        {
            if (world != null)
                return world;
            var ark = ArkSaveEditor.Deserializer.ArkSaveDeserializer.OpenDotArk();
            //Get normal object
            world = new ArkWorld(ark);
            return world;
        }

        public static Task MainAsync()
        {
            var host = new WebHostBuilder()
                .UseKestrel(options =>
                {
                    IPAddress addr = IPAddress.Any;
                    options.Listen(addr, 43298);
                    /*options.Listen(addr, 443, listenOptions =>
                    {
                        listenOptions.UseHttps(LibRpwsCore.config.ssl_cert_path, "");
                    });*/

                })
                .UseStartup<Program>()
                .Build();

            return host.RunAsync();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.Run(OnHttpRequest);

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

        public static Task QuickWriteJsonToDoc<T>(Microsoft.AspNetCore.Http.HttpContext context, T data, int code = 200)
        {
            return QuickWriteToDoc(context, JsonConvert.SerializeObject(data, Formatting.Indented), "application/json", code);
        }

        public static string GenerateRandomString(int length)
        {
            string output = "";
            char[] chars = "qwertyuiopasdfghjklzxcvbnm1234567890QWERTYUIOPASDFGHJKLZXCVBNM".ToCharArray();
            Random rand = new Random();
            for (int i = 0; i < length; i++)
            {
                output += chars[rand.Next(0, chars.Length)];
            }
            return output;
        }

        
    }
}
