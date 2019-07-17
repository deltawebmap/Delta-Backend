using ArkWebMapLauncherProviders.NetEntities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace ArkWebMapLauncherProviders
{
    class Program
    {
        public const int LAUNCHER_VERSION = 0;

        public static Process mainProcess;

        public static LauncherConfig launcherConfig;
        public static ProvidersConfigFile providersConfig;

        public static ArkManagerServer[] serverConfigs;
        public static Dictionary<string, Process> serverProcesses = new Dictionary<string, Process>();

        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to ArkWebMap for Providers! Getting ready...");

            //Open config file
            Console.WriteLine("Reading providers config file...");
            if(File.Exists(GetRelPath("providers_config.json")))
                providersConfig = JsonConvert.DeserializeObject<ProvidersConfigFile>(File.ReadAllText(GetRelPath("providers_config.json")));
            else
            {
                Console.WriteLine("Providers config file did not exist, or it wasn't placed in the correct directory. Exiting when you press enter.");
                Console.ReadLine();
                return;
            }

            //Download the config file
            Console.WriteLine("Downloading remote config file...");
            LauncherConfig config;
            try
            {
                config = DownloadClass<LauncherConfig>($"https://ark.romanport.com/config/games/0/launcher_config.json?is_windows={GetIsWindows().ToString()}&launcher_version={LAUNCHER_VERSION}&launcher_type=PROVIDERS&is_bg=false");
                launcherConfig = config;
            }
            catch (Exception ex)
            {
                ExitWithFatalError("Failed to download launcher config file. Are you online?");
                return;
            }

            //Now, load the package info if it exists
            PackageInfo currentPackage = null;
            if (File.Exists(GetRelPath("subserver_packageinfo.json")))
                currentPackage = JsonConvert.DeserializeObject<PackageInfo>(File.ReadAllText(GetRelPath("subserver_packageinfo.json")));

            //If the package is missing, download a new release
            if(currentPackage == null)
                UpdaterTool.Update(config, out currentPackage);
            else
            {
                //If the current package is out of date, update
                if(currentPackage.version < config.latest_subserver.version)
                    UpdaterTool.Update(config, out currentPackage);
            }

            //Download providers config file, unique for this machine
            Console.WriteLine("Downloading remote machine config file...");
            InternalMachineConfigResponse machineConfig;
            try
            {
                machineConfig = DownloadClass<InternalMachineConfigResponse>($"https://ark.romanport.com/api/providers/internal/machine_config", new Dictionary<string, string>
                {
                    {"X-Api-Token", providersConfig.api_token },
                    {"X-Machine-ID", providersConfig.machine_id }
                });
            }
            catch (Exception ex)
            {
                ExitWithFatalError("Failed to download the machine config file. DeltaWebMap servers might be down temporarily, or your API token might be incorrect.");
                return;
            }

            //Write some vars
            Console.WriteLine($"Welcome, {machineConfig.profile.name}! This is machine {machineConfig.machine.name} ({machineConfig.machine._id}) at {machineConfig.machine.location}.");
            serverConfigs = machineConfig.servers;

            //Now, execute it
            Console.WriteLine("Starting servers...");
            foreach(var s in serverConfigs)
            {
                StartMainService(currentPackage, s, machineConfig);
            }
            Console.WriteLine("All servers are now running.");

            //Start background worker
            //BackgroundWorker.StartBackgroundWorker(config, currentPackage);

            /*
            while(true)
            {
                mainProcess.WaitForExit();

                //Exited. Check if expected
                if (!UpdaterTool.isQuittingForUpdate)
                    break; //Unexpected

                //This was expected. Wait for the process to restart
                while (UpdaterTool.isQuittingForUpdate)
                    Thread.Sleep(500);
            }
            ExitWithFatalError("Subserver shut down.");*/

            //Wait indefinitely 
            while (true)
                Console.ReadLine();
        }

        public static void StartMainService(PackageInfo currentPackage, ArkManagerServer currentServer, InternalMachineConfigResponse machineConfig)
        {
            //Log
            Console.WriteLine($"Starting server {currentServer._id} [{currentServer.name}]...");

            //Create config directory
            string serverRoot = GetRelPath("server_data/" + currentServer._id + "/");
            if(!Directory.Exists(serverRoot))
            {
                Console.WriteLine($"Data directory does not exist for {currentServer._id} [{currentServer.name}]. It is being created...");
                Directory.CreateDirectory(serverRoot);
            }

            //Update files.
            string settingsPath = serverRoot + "subserver_settings.json";
            if (File.Exists(settingsPath))
                File.Delete(settingsPath);
            File.WriteAllText(settingsPath, ProduceSubserverConfigFile(currentPackage, currentServer, machineConfig));

            //Start process
            string processPath = GetRelPath("latest_subserver/" + currentPackage.binary_pathname);
            if (!File.Exists(processPath))
            {
                ExitWithFatalError("Subserver binary does not exist or was not found. Try deleting server_packageinfo.json");
                return;
            }

            //Create launch options
            LaunchOptions lo = new LaunchOptions
            {
                launcher_version = LAUNCHER_VERSION,
                path_config = settingsPath,
                path_db = serverRoot + "subserver_userdb.db",
                path_root = serverRoot
            };

            //Create start config
            ProcessStartInfo pi = new ProcessStartInfo
            {
                WorkingDirectory = GetRelPath("latest_subserver/"),
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = processPath,
                Arguments = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(lo))),
                RedirectStandardOutput = true
            };

            //Start
            Process p = new Process();
            p.StartInfo = pi;
            p.Start();
            if (!serverProcesses.ContainsKey(currentServer._id))
                serverProcesses.Add(currentServer._id, p);
            else
                Console.WriteLine("TODO: Handle"); //TODO!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

            //Log
            Console.WriteLine($"Server {currentServer._id} [{currentServer.name}] was started.");
        }

        public static string ProduceSubserverConfigFile(PackageInfo currentPackage, ArkManagerServer currentServer, InternalMachineConfigResponse machineConfig)
        {
            //First, clone the base config. This is 100% a hack, but IT WORKS!
            ArkSlaveConfig ssc = JsonConvert.DeserializeObject<ArkSlaveConfig>(JsonConvert.SerializeObject(launcherConfig.base_subserver_config));

            //Now, copy the map and path
            ssc.child_config.save_location = currentServer.game_settings.map_path;
            ssc.child_config.save_map = currentServer.game_settings.map_name;

            //Grab the creds and copy them
            InternalMachineConfigResponseServerInfo linkedCurrentServer = machineConfig.linked_servers[currentServer.linked_id];
            ssc.auth = new ArkSlaveConnection
            {
                creds = Convert.ToBase64String(linkedCurrentServer.creds),
                id = linkedCurrentServer.id
            };

            //Now, stringify and return
            return JsonConvert.SerializeObject(ssc);
        }

        public static void ExitWithFatalError(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("FATAL ERROR: " + msg);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Exiting!");
            Thread.Sleep(5000);
            Environment.Exit(1);
        }

        public static LauncherConfig_SubserverReleaseBinary GetPlatformBinary(LauncherConfig_SubserverRelease release)
        {
            //Get platform
            bool isWindows = GetIsWindows();

            //Choose binary
            if (isWindows)
                return release.binaries["windows"];
            else
                return release.binaries["linux"];
        }

        public static bool GetIsWindows()
        {
            return System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows);
        }

        public static string GetRelPath(string ext)
        {
            return AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\').TrimEnd('/') + "/" + ext;
        }

        public static T DownloadClass<T>(string url, Dictionary<string, string> headers = null)
        {
            T returned;
            using (WebClient wc = new WebClient())
            {
                if(headers != null)
                {
                    foreach (var h in headers)
                        wc.Headers.Add(h.Key, h.Value);
                }
                string data = wc.DownloadString(url);
                returned = JsonConvert.DeserializeObject<T>(data);
            }
            return returned;
        }
    }
}
