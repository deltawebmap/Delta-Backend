using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace ArkWebMapLauncher
{
    class Program
    {
        public const int LAUNCHER_VERSION = 0;

        public static Process mainProcess;

        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to ArkWebMap! Getting ready...");
            Console.WriteLine("Downloading config file...");

            //Download the config file
            LauncherConfig config;
            try
            {
                config = DownloadClass<LauncherConfig>($"https://ark.romanport.com/launcher_config.json?is_windows={GetIsWindows().ToString()}&launcher_version={LAUNCHER_VERSION}&launcher_type=BLUE&is_bg=false");
            } catch (Exception ex)
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

            //Now, execute it
            Console.WriteLine("Starting...");
            StartMainService(currentPackage);

            //Start background worker
            BackgroundWorker.StartBackgroundWorker(config, currentPackage);

            //Wait
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
            ExitWithFatalError("Subserver shut down.");
        }

        public static void StartMainService(PackageInfo currentPackage)
        {
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
                path_config = GetRelPath("subserver_settings.json"),
                path_db = GetRelPath("subserver_userdb.db"),
                path_root = GetRelPath("")
            };

            //Create start config
            ProcessStartInfo pi = new ProcessStartInfo
            {
                WorkingDirectory = GetRelPath("latest_subserver/"),
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = processPath,
                Arguments = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(lo)))
            };

            mainProcess = new Process();
            mainProcess.StartInfo = pi;
            mainProcess.Start();
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

        public static T DownloadClass<T>(string url)
        {
            T returned;
            using (WebClient wc = new WebClient())
            {
                string data = wc.DownloadString(url);
                returned = JsonConvert.DeserializeObject<T>(data);
            }
            return returned;
        }
    }
}
