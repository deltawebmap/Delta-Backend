using ArkBridgeSharedEntities.Entities;
using ArkBridgeSharedEntities.Entities.RemoteConfig;
using ArkBridgeSharedEntities.Requests;
using ArkHttpServer;
using ArkSaveEditor.World;
using ArkWebMapLightspeedClient;
using ArkWebMapSlaveServer.NetEntities;
using LiteDB;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace ArkWebMapSlaveServer
{
    public class ArkWebMapServer
    {
        public static ArkSlaveConfig config;
        public static RemoteConfigFile remote_config;
        public static string config_path;
        public static Random rand = new Random();
        public static Timer reportTimer;
        public static Timer offlineDataTimer;
        public static byte[] creds;
        public static LiteDatabase db;
        public static AWMLightspeedClient lightspeed;

        public const int MY_VERSION = 1;

        public static Task MainAsync(ArkSlaveConfig config, string configPath, RemoteConfigFile remote_config, string dbPath)
        {
            Console.WriteLine("Contacting master server...");
            ArkWebMapServer.config = config;
            ArkWebMapServer.remote_config = remote_config;
            config_path = configPath;
            ArkWebMapServer.creds = Convert.FromBase64String(config.auth.creds);
            if (!HandshakeMasterServer(""))
                return null;

            Console.WriteLine("Opening database...");
            db = new LiteDatabase(dbPath);

            //Get LIGHTSPEED config
            LightspeedConfigFile lightspeedConfig = AWMLightspeedClient.GetConfigFile();

            Console.WriteLine("Configurating internal server...");
            string apiPrefix = lightspeedConfig.client_endpoint_prefix.Replace("{serverId}", config.auth.id).Replace("{serverGame}", 0.ToString());
            ArkWebServer.Configure(config.child_config, apiPrefix, (int tribeId, TribeNotification n) =>
            {
                //Create payload and send it.
                UserNotificationRequest r = new UserNotificationRequest
                {
                    notification = n,
                    tribeId = tribeId
                };

                //Transmit
                MasterServer.SendRequestToMaster<TrueFalseReply>("send_tribe_notification", r);
            }, (string action, object request, Type t) =>
            {
                return MasterServer.SendRequestToMaster(action, request, t);
            }, db);

            //Submit world report
            Console.WriteLine("Submitting ARK world report to master server...");
            SendWorldReport();

            //Connect to the LIGHTSPEED network.
            Console.WriteLine("Connecting...");
            lightspeed = AWMLightspeedClient.CreateClient(config.auth.id, config.auth.creds, 0, HttpHandler.OnHttpRequest, true);

            //Submit offline data
            Console.WriteLine("Submitting ARK offline data to master server...");
            SendOfflineData();

            //Set timer to transmit the world report every five minutes
            reportTimer = new Timer(TimeSpan.FromMinutes(5).TotalMilliseconds);
            reportTimer.AutoReset = true;
            reportTimer.Elapsed += ReportTimer_Elapsed;
            reportTimer.Start();

            //Set timer to transmit the world offline data every 15 minutes
            offlineDataTimer = new Timer(TimeSpan.FromMinutes(15).TotalMilliseconds);
            offlineDataTimer.AutoReset = true;
            offlineDataTimer.Elapsed += OfflineDataTimer_Elapsed;
            offlineDataTimer.Start();

            //If we're in debug mode, warn
            if (config.debug_mode)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("WARNING! This server is running in debug mode. Debug mode enables additional logging and disables security checks for incoming requests. THIS MEANS ANYONE CAN REQUEST ANY DATA ON THIS SERVER!");
                Console.ForegroundColor = ConsoleColor.White;
            }

            return Task.Delay(-1);
        }

        private static void ReportTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            //Transmit report
            Console.WriteLine("Updating master server with ARK world report...");
            if(!SendWorldReport())
            {
                //Error!
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Failed to update master server with world report! New users will not be authenticated.");
                Console.ForegroundColor = ConsoleColor.White;
            } else
            {
                Console.WriteLine("Master server ARK world report is now up to date.");
            }
        }

        private static void OfflineDataTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            //Transmit report
            Console.WriteLine("Updating master server with ARK offline data...");
            if (!SendWorldReport())
            {
                //Error!
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Failed to update offline data.");
                Console.ForegroundColor = ConsoleColor.White;
            }
            else
            {
                Console.WriteLine("Master server ARK offline data is now up to date.");
            }
        }

        public static void Log(string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static bool SendWorldReport()
        {
            ArkSlaveReport report = WorldReportBuilder.GenerateTribeOverview();

            //Send
            TrueFalseReply report_reply;
            try
            {
                report_reply = MasterServer.SendRequestToMaster<TrueFalseReply>("world_report", report);
            }
            catch (Exception ex)
            {
                return false;
            }
            if (report_reply.ok == false)
            {
                return false;
            }
            return true;
        }

        public static bool SendOfflineData()
        {
            try
            {
                OfflineDataReportBuilder.SendOfflineData();
                return true;
            } catch (Exception ex)
            {
                return false;
            }
        }

        public static bool HandshakeMasterServer(string mapName)
        {
            //Connect to the master server and download data.
            SlaveHelloPayload request = new SlaveHelloPayload
            {
                my_port = config.web_port,
                my_version = MY_VERSION,
                my_ark_map = mapName
            };

            //Send
            SlaveHelloReply reply;
            try
            {
                reply = MasterServer.SendRequestToMaster<SlaveHelloReply>("hello", request);
            } catch (Exception ex)
            {
                Console.Clear();
                if(ex.Message == "Server reply was not valid.")
                    Console.WriteLine("Could not communicate with the master server. \n\nThe server is probably down for maintenance. Try again later.");
                Console.ReadLine();
                return false;
            }

            //Check
            if (reply.status == SlaveHelloReply_MessageType.Ok)
                return true; //OK
            else if (reply.status == SlaveHelloReply_MessageType.SlaveOutOfDate)
            {
                Console.Clear();
                Console.WriteLine("Could not connect to master server! This version is too out of date.\n\n" + reply.status_info["notes"] + "\n\nPress ENTER to download the new version!");
                Console.ReadLine();
                System.Diagnostics.Process.Start(reply.status_info["download_url"]);
            }
            else if (reply.status == SlaveHelloReply_MessageType.ServerDeleted)
            {
                Console.Clear();
                Console.WriteLine("This server was deleted from the web interface. You will need to set up ArkWebMap again if you would like to continue using it.\n\nThe program will now close. Run it again to restart the setup process.");
                Console.ReadLine();
                File.Delete(config_path);
                return false;
            }
            else
            {
                Console.Clear();
                Console.WriteLine("Could not connect to master server! It returned an unknown reply. You're probably very out of date.");
                Console.ReadLine();
            }

            return false;
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

        public static Task QuickWriteJsonToDoc<T>(Microsoft.AspNetCore.Http.HttpContext context, T data, int code = 200)
        {
            return QuickWriteToDoc(context, JsonConvert.SerializeObject(data, Formatting.Indented), "application/json", code);
        }

        public static string GenerateRandomString(int length)
        {
            string output = "";
            char[] chars = "qwertyuiopasdfghjklzxcvbnm1234567890QWERTYUIOPASDFGHJKLZXCVBNM".ToCharArray();
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

        public static bool CompareByteArrays(byte[] b1, byte[] b2)
        {
            if (b1.Length != b2.Length)
                return false;
            for (int i = 0; i < b1.Length; i++)
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
