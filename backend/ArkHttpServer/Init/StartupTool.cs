using ArkBridgeSharedEntities.Entities.RemoteConfig;
using ArkHttpServer.Entities;
using ArkHttpServer.Init.OfflineData;
using ArkHttpServer.Init.WorldReport;
using ArkHttpServer.Tools;
using ArkHttpServer.Tools.FoodSim;
using ArkSaveEditor;
using ArkSaveEditor.Entities;
using ArkWebMapGatewayClient;
using ArkWebMapLightspeedClient;
using LiteDB;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;

namespace ArkHttpServer.Init
{
    public static class StartupTool
    {
        public const int CURRENT_RELEASE_ID = 2;

        /// <summary>
        /// Called when the application starts
        /// </summary>
        /// <param name="args"></param>
        public static bool OnStartup(string[] args)
        {
            //Obtain options
            LaunchOptions launchOptions = GetLaunchOptions(args);
            RemoteConfigFile config = GetRemoteConfig();
            if (config == null)
                return false;

            //Check if we're configured
            if(!File.Exists(launchOptions.path_config))
            {
                //We need to configure
                if (SetupTool.StartSetup(launchOptions, config))
                {

                } else
                {
                    return false;
                }
            }

            //Start
            return Prepare(launchOptions, config);
        }

        /// <summary>
        /// Called when we have all of the options we need.
        /// </summary>
        /// <returns></returns>
        public static bool Prepare(LaunchOptions options, RemoteConfigFile masterConfig)
        {
            //Open the subserver config
            ServerConfigFile config = JsonConvert.DeserializeObject<ServerConfigFile>(File.ReadAllText(options.path_config));

            //Open the database
            ArkWebServer.db = new LiteDatabase(options.path_db);

            //Set
            ArkWebServer.remote_config = masterConfig;
            ArkWebServer.config = config;

            //Setup lightspeed
            LightspeedConfigFile lightspeedConfig = AWMLightspeedClient.GetConfigFile();
            string apiPrefix = lightspeedConfig.client_endpoint_prefix.Replace("{serverId}", config.connection.id).Replace("{serverGame}", 0.ToString());
            ArkWebServer.api_prefix = apiPrefix;

            //Import PrimalData
            ImportPrimalData(masterConfig, options.path_root + "primal_data.pdp");

            //Send world report
            if (!WorldReportBuilder.SendWorldReport())
                return false;

            //Connect to the LIGHTSPEED network.
            Console.WriteLine("Connecting to lightspeed...");
            ArkWebServer.lightspeed = AWMLightspeedClient.CreateClient(config.connection.id, config.connection.creds, 0, ArkWebServer.OnHttpRequest, false);

            //Connect to the GATEWAY
            Console.WriteLine("Connecting to gateway...");
            ArkWebServer.gateway_handler = new Gateway.GatewayHandler();
            ArkWebServer.gateway = AWMGatewayClient.CreateClient(GatewayClientType.SubServer, "sub-server", "data_version/" + ClientVersion.DATA_VERSION, ClientVersion.VERSION_MAJOR, ClientVersion.VERSION_MINOR, false, ArkWebServer.gateway_handler, $"{config.connection.id}#{config.connection.creds}");

            //Send tiles
            DynamicTileUploader.UploadAll(WorldLoader.GetWorld()).GetAwaiter().GetResult();

            //Send offline data
            OfflineDataBuilder.SendOfflineData();

            //Set timer for checking when the map was updated
            ArkWebServer.event_checker_timer = new System.Timers.Timer(5000);
            ArkWebServer.event_checker_timer.Elapsed += ArkWebServer.Event_checker_timer_Elapsed;
            ArkWebServer.event_checker_timer.Start();

            return true;
        }

        /// <summary>
        /// Imports primal data, or downloads new data if it is out of date
        /// </summary>
        /// <param name="pathname"></param>
        /// <returns></returns>
        private static void ImportPrimalData(RemoteConfigFile config, string pathname)
        {
            //Check if this pathname exists
            if(File.Exists(pathname))
            {
                try
                {
                    //Open stream and begin reading
                    using (FileStream fs = new FileStream(pathname, System.IO.FileMode.Open, FileAccess.Read))
                    {
                        //Load package
                        bool ok = ArkImports.ImportContentFromPackage(fs, (PrimalDataPackageMetadata metadata) =>
                        {
                            //Verify this is up to date.
                            return (config.primal_data.version_minor == metadata.version_minor) && (config.primal_data.version_major == metadata.version_major);
                        });

                        //Stop if this was okay, else continue
                        if (ok)
                            return;
                    }
                } catch (Exception ex)
                {
                    //We'll fail and redownload.
                    Console.WriteLine("Failed to read Primal Data: " + ex.Message);
                }

                //If we landed here, this version is out of date. Remove
                File.Delete(pathname);
            }

            //We'll need to download an updated release.
            Console.WriteLine("Primal Data is out of date. Updating...");
            using (WebClient wc = new WebClient())
                wc.DownloadFile(config.primal_data.download_url, pathname);
            Console.WriteLine("Primal Data was updated. Opening...");

            //It's gross, but we call ourselves.
            ImportPrimalData(config, pathname);
        }

        /// <summary>
        /// Obtains the launch options for this application.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private static LaunchOptions GetLaunchOptions(string[] args)
        {
            //If the config filename was passed as an arg, use it. Else, assume debug enviornment
            LaunchOptions launchOptions;
            if (args.Length >= 1)
            {
                string configJson = Encoding.UTF8.GetString(Convert.FromBase64String(args[0]));
                launchOptions = JsonConvert.DeserializeObject<LaunchOptions>(configJson);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("WARNING: No settings path was passed into the program. This is not being run inside of the usual launcher enviornment. Assuming this is a debug enviornment...");
                launchOptions = new LaunchOptions
                {
                    launcher_version = -1,
                    path_config = "debug_net_config.json",
                    path_db = "debug_userdb.db",
                    path_root = "./"
                };
                Console.ForegroundColor = ConsoleColor.White;
            }
            return launchOptions;
        }

        private static RemoteConfigFile GetRemoteConfig()
        {
            //Request the remote config file
            try
            {
                using (WebClient wc = new WebClient())
                {
                    return JsonConvert.DeserializeObject<RemoteConfigFile>(wc.DownloadString($"https://config.deltamap.net/prod/games/0/client_config.json?client=subserver&version={CURRENT_RELEASE_ID.ToString()}"));
                }
            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Failed to download remote config file. Please try again later. Error code -1.");
                Console.ReadLine();
                return null;
            }
        }
    }
}
