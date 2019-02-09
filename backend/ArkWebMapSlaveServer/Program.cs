using ArkBridgeSharedEntities.Entities;
using ArkHttpServer;
using ArkSaveEditor.World;
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
        public static string config_path;
        public static Random rand = new Random();
        public static Timer reportTimer;
        public static byte[] creds;

        public const int MY_VERSION = 1;

        public static Task MainAsync(ArkSlaveConfig config, string configPath)
        {
            Console.WriteLine("Contacting master server...");
            ArkWebMapServer.config = config;
            config_path = configPath;
            ArkWebMapServer.creds = Convert.FromBase64String(config.auth.creds);
            if (!HandshakeMasterServer(""))
                return null;

            Console.WriteLine("Configurating internal server...");
            ArkWebServer.Configure(config.child_config, "https://ark.romanport.com/api/servers/"+config.auth.id);

            //Submit world report
            Console.WriteLine("Submitting ARK world report to master server...");
            SendWorldReport();

            //Start server
            Console.WriteLine("Starting HTTP server...");
            var host = new WebHostBuilder()
                .UseKestrel(options =>
                {
                    IPAddress addr = IPAddress.Any;
                    options.Listen(addr, config.web_port);

                })
                .UseStartup<ArkWebMapServer>()
                .Build();

            //Set timer to transmit the world report every five minutes
            reportTimer = new Timer(TimeSpan.FromMinutes(5).TotalMilliseconds);
            reportTimer.AutoReset = true;
            reportTimer.Elapsed += ReportTimer_Elapsed;
            reportTimer.Start();

            return host.RunAsync();
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

        public static void Log(string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static bool SendWorldReport()
        {
            //Get world
            ArkWorld w = WorldLoader.GetWorld(out DateTime time);
            ArkSlaveReport report = new ArkSlaveReport();
            report.lastSaveTime = time;
            report.accounts = new System.Collections.Generic.List<ArkSlaveReport_PlayerAccount>();
            foreach (var player in w.players)
                report.accounts.Add(new ArkSlaveReport_PlayerAccount
                {
                    player_name = player.playerName,
                    allow_player = true,
                    player_steam_id = player.steamId,
                    player_tribe_id = player.tribeId,
                    player_tribe_name = player.playerName
                });
            report.map_name = w.map;
            report.map_time = w.gameTime;

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
                    Console.WriteLine("Could not connect to master server! A connection error occurred. \n\nThe server is probably down for maintenance. Try again later.");
                else
                    Console.WriteLine("Could not connect to master server! A connection error occurred: \n\n" + ex.Message + ex.StackTrace + "\n\nThe server is probably down for maintenance. Try again later.");
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
