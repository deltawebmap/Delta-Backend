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
    public partial class ArkWebServer
    {
        public static ServerConfigFile config;

        public static System.Timers.Timer event_checker_timer;

        public static string api_prefix;

        public const int EVENTS_HEARTRATE = 10000; //How often event requests come in.

        public static void Configure(ServerConfigFile config, string api_prefix)
        {
            //Load
            ArkWebServer.config = config;
            ArkWebServer.api_prefix = api_prefix;

            //Load save editor entries
            ArkImports.ImportContent(@"PrimalData/world.json", @"PrimalData/dinos.json", @"PrimalData/items.json");

            //Load map
            WorldLoader.GetWorld();

            //Start event checker timer
            event_checker_timer = new System.Timers.Timer(5000);
            event_checker_timer.Elapsed += Event_checker_timer_Elapsed;
            event_checker_timer.Start();
        }

        private static void Event_checker_timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //Check all of our sessions for updated files.
            HttpServices.EventService.AddEvent(new Entities.HttpSessionEvent(null, Entities.HttpSessionEventType.TestEvent));
            
            //Check for map updates
            if(WorldLoader.CheckForMapUpdates())
            {
                HttpServices.EventService.AddEvent(new Entities.HttpSessionEvent(null, Entities.HttpSessionEventType.MapUpdate));
            }
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
