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
using ArkBridgeSharedEntities.Requests;
using LiteDB;
using ArkWebMapLightspeedClient;
using ArkBridgeSharedEntities.Entities.RemoteConfig;
using ArkHttpServer.Init.WorldReport;
using ArkHttpServer.Init.OfflineData;

namespace ArkHttpServer
{
    public partial class ArkWebServer
    {
        public static System.Timers.Timer event_checker_timer;

        public static string api_prefix;

        public static LiteDatabase db;
        public static AWMLightspeedClient lightspeed;
        public static ServerConfigFile config;
        public static RemoteConfigFile remote_config;

        public static DateTime lastOfflineReport;

        public static bool CheckPermission(string name)
        {
            return config.base_permissions.CheckPermission(name);
        }

        public static void Log(string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void Event_checker_timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //Check all of our sessions for updated files.
            //HttpServices.EventService.AddEvent(new Entities.HttpSessionEvent(null, Entities.HttpSessionEventType.TestEvent));
            
            //Check for map updates
            if(WorldLoader.CheckForMapUpdates())
            {
                //TODO: Notify map update

                //We're going to send the Ark world report
                WorldReportBuilder.SendWorldReport();

                //If it's been over some number of minutes since the last offline report, send it
                TimeSpan timeSinceLastReport = DateTime.UtcNow - lastOfflineReport;
                if (timeSinceLastReport.TotalSeconds > remote_config.sub_server_config.offline_sync_policy_seconds)
                    OfflineDataBuilder.SendOfflineData();
            }

            //Find ALL baby dinos and check their stats so we can send notifications to mobile clients
            MobileNotificationsEngine.BabyDinoNotifications.CheckBabyDinos();
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

        public static RequestHttpMethod FindRequestMethod(LightspeedRequest context)
        {
            return Enum.Parse<RequestHttpMethod>(context.method.ToLower());
        }

        public static T DecodePostBody<T>(LightspeedRequest context)
        {
            string buf = Encoding.UTF8.GetString(context.body);
            return JsonConvert.DeserializeObject<T>(buf);
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
