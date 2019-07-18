using ArkWebMapSlaveServer;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Net.Http;
using ArkBridgeSharedEntities.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Net.Sockets;
using System.Threading.Tasks;
using ArkBridgeSharedEntities.Entities.RemoteConfig;

namespace ArkWebMapSlaveServerConsole
{
    /// <summary>
    /// Nothing but a frontend console for the code.
    /// </summary>
    partial class Program
    {
        const int SETUP_VERSION = 1;
        const float CURRENT_RELEASE_ID = 1.1f;

        static RemoteConfigFile remote_config;
        static LaunchOptions launchOptions;

        static void Main(string[] args)
        {
            //If the config filename was passed as an arg, use it. Else, assume debug enviornment
            if(args.Length >= 1)
            {
                string configJson = Encoding.UTF8.GetString(Convert.FromBase64String(args[0]));
                launchOptions = JsonConvert.DeserializeObject<LaunchOptions>(configJson);
            } else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("WARNING: No settings path was passed into the program. This is not being run inside of the usual launcher enviornment. Assuming this is a debug enviornment...");
                launchOptions = new LaunchOptions
                {
                    launcher_version = -1,
                    path_config = "debug_net_config.json",
                    path_db = "debug_userdb.db",
                    path_root = "../"
                };
                Console.ForegroundColor = ConsoleColor.White;
            }
            
            //Request the remote config file
            Console.WriteLine("Downloading remote configuration file...");
            try
            {
                using (WebClient wc = new WebClient())
                {
                    remote_config = JsonConvert.DeserializeObject<RemoteConfigFile>(wc.DownloadString($"https://config.deltamap.net/prod/games/0/client_config.json?client=subserver&version={CURRENT_RELEASE_ID.ToString()}"));
                }
            } catch
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Failed to download remote config file. Please try again later.");
                Console.ReadLine();
                return;
            }
            
            //If the config file exists, jump right to running it.
            if (File.Exists(GetConfigPath()))
            {
                Run();
            }
            else
            {
                //Not set up. Show prompt and ask for info
                clientSessionId = IntroAndPromptUserForCode();

                //Now that we have the code, we can start to do setup. 
                //Send ready
                SendMasterMessage(new ArkSetupProxyMessage
                {
                    data = new Dictionary<string, string>
                    {
                        {"setup_version",SETUP_VERSION.ToString() }
                    },
                    type = ArkSetupProxyMessage_Type.ServerHello
                });

                //Begin checking for events and responding to them.
                bool continueLoop = true;
                while(continueLoop)
                {
                    List<ArkSetupProxyMessage> messages = GetMasterMessages();
                    foreach (ArkSetupProxyMessage message in messages)
                    {
                        Console.WriteLine(JsonConvert.SerializeObject(message));
                        switch (message.type)
                        {
                            case ArkSetupProxyMessage_Type.WebClientRequestServerTestPort:
                                TcpTester.OnBeginRequest(message);
                                break;
                            case ArkSetupProxyMessage_Type.CheckArkFile:
                                MapFileTester.OnBeginRequest(message);
                                break;
                            case ArkSetupProxyMessage_Type.UploadConfigAndFinish:
                                //Deserialize and save
                                File.WriteAllText(GetConfigPath(), message.data["config"]);

                                //Respond
                                SendMasterMessage(new ArkSetupProxyMessage
                                {
                                    data = new Dictionary<string, string>(),
                                    type = ArkSetupProxyMessage_Type.ServerGoodbye
                                });
                                continueLoop = false;
                                break;
                        }
                    }
                    Thread.Sleep(1000);
                }
                Console.Clear();
                DrawWithColor("Finished! Starting...", ConsoleColor.Blue);
                Console.ForegroundColor = ConsoleColor.White;
                Run();
            }
            
        }



        

        static string clientSessionId;

        static List<ArkSetupProxyMessage> GetMasterMessages()
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(GetProxyEndpoint());
                request.Method = "GET";
                var response = (HttpWebResponse)request.GetResponse();
                string responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                return JsonConvert.DeserializeObject<List<ArkSetupProxyMessage>>(responseString); 
            } catch
            {
                return new List<ArkSetupProxyMessage>();
            }
        }

        public static void SendMasterMessage(ArkSetupProxyMessage message)
        {
            string ser_string = JsonConvert.SerializeObject(message);
            byte[] ser = Encoding.UTF8.GetBytes(ser_string);
            var request = (HttpWebRequest)WebRequest.Create(GetProxyEndpoint());

            request.Method = "POST";
            request.ContentLength = ser.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(ser, 0, ser.Length);
            }

            var response = (HttpWebResponse)request.GetResponse();

            if(response.StatusCode != HttpStatusCode.OK)
            {
                DrawWithColor("Failed to send master server message. Are you online? ", ConsoleColor.Red);
                Console.ReadLine();
                throw new Exception();
            }
            //Assume response is OK if we did not get an error
        }

        public static string GetProxyEndpoint()
        {
            return remote_config.sub_server_config.endpoints.server_setup_proxy.Replace("{clientSessionId}", clientSessionId);
        }

        static string GetConfigPath()
        {
            return launchOptions.path_config;
        }

        static void Run()
        {
            string config_path = GetConfigPath();
            ArkSlaveConfig config = JsonConvert.DeserializeObject<ArkSlaveConfig>(File.ReadAllText(config_path));
            Task t = ArkWebMapServer.MainAsync(config, config_path, remote_config, launchOptions.path_db);
            if(t != null)
                t.GetAwaiter().GetResult();
        }

        static void DrawWithColor(string message, ConsoleColor foreground = ConsoleColor.White, ConsoleColor background = ConsoleColor.Black)
        {
            Console.ForegroundColor = foreground;
            Console.BackgroundColor = background;
            Console.Write(message);
        }
    }
}
