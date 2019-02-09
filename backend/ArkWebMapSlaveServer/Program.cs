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

namespace ArkWebMapSlaveServer
{
    public class ArkWebMapServer
    {
        public static ArkSlaveConfig config;
        public static Random rand = new Random();

        public const int MY_VERSION = 1;

        public static Task MainAsync(ArkSlaveConfig config)
        {
            Console.WriteLine("Contacting master server...");
            ArkWebMapServer.config = config;
            if (!HandshakeMasterServer(""))
                return null;

            Console.WriteLine("Configurating internal server...");
            ArkWebServer.Configure(config.child_config, "https://ark.romanport.com/api/servers/"+config.auth.id);

            Console.WriteLine("Starting HTTP server...");
            var host = new WebHostBuilder()
                .UseKestrel(options =>
                {
                    IPAddress addr = IPAddress.Any;
                    options.Listen(addr, config.web_port);

                })
                .UseStartup<ArkWebMapServer>()
                .Build();

            //Open the Ark world to submit some data about us.
            Console.WriteLine("Opening ARK world...");
            ArkWorld w = new ArkWorld(ArkSaveEditor.Deserializer.ArkSaveDeserializer.OpenDotArk(config.child_config.save_location));
            ArkSlaveReport report = new ArkSlaveReport();
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

            //Submit
            Console.WriteLine("Submitting ARK world report to master server...");
            TrueFalseReply report_reply;
            try
            {
                report_reply = MasterServer.SendRequestToMaster<TrueFalseReply>("world_report", report);
            }
            catch (Exception ex)
            {
                Console.Clear();
                if (ex.Message == "Server reply was not valid.")
                    Console.WriteLine("Could not connect to master server! A connection error occurred. \n\nThe server is probably down for maintenance. Try again later.");
                else
                    Console.WriteLine("Could not connect to master server! A connection error occurred: \n\n" + ex.Message + ex.StackTrace + "\n\nThe server is probably down for maintenance. Try again later.");
                Console.ReadLine();
                return null;
            }
            if(report_reply.ok == false)
            {
                Console.WriteLine("Could not connect to master server! The master server failed to process our report.");
                Console.ReadLine();
                return null;
            }

            return host.RunAsync();
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
                Console.WriteLine("Could not connect to master server! This version is too out of date.\n\n"+reply.status_info["notes"]+"\n\nPress ENTER to download the new version!");
                Console.ReadLine();
                System.Diagnostics.Process.Start(reply.status_info["download_url"]);
            } else
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
