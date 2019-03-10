using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;

namespace ArkWebMapSubServerUpdater
{
    class Program
    {
        static void Main(string[] args)
        {
            string output = args[0];

            //Download remote config
            StartStepWrite("Downloading config file...");
            RemoteConfigFile remote_config;
            using (WebClient wc = new WebClient())
            {
                remote_config = JsonConvert.DeserializeObject<RemoteConfigFile>(wc.DownloadString("https://ark.romanport.com/client_config.json?client=subserver_updater&version=1"));
            }

            //Determine if this is a Windows or Linux machine.
            bool isWindows = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows);
            RemoteConfigFile_Release_Binary binary;
            if (isWindows)
                binary = remote_config.latest_release.binaries["windows"];
            else
                binary = remote_config.latest_release.binaries["linux"];

            //Download the update package.
            StartStepWrite("Done.");
            StartStepWrite("Downloading update package...");
            byte[] updater_binary;
            try
            {
                using (WebClient wc = new WebClient())
                    updater_binary = wc.DownloadData(binary.url);
            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("Failed. Press enter to exit.");
                Console.ReadLine();
                return;
            }
            StartStepWrite("Done.");

            //Unzip
            StartStepWrite("Unzipping...");
            using (MemoryStream ms = new MemoryStream(updater_binary))
            {
                using (ZipArchive za = new ZipArchive(ms, ZipArchiveMode.Read))
                {
                    za.ExtractToDirectory(output, true);
                }
            }
            StartStepWrite("Done.");
            Console.Clear();

            //Restart program
            StartStepWrite("Restarting...");
            Process.Start(new ProcessStartInfo
            {
                WorkingDirectory = output,
                FileName = output+binary.exe_name
            });
        }

        static void FailWrite(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(message);
        }

        static void StartStepWrite(string message)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("\n[Helper] "+message);
        }
    }
}
