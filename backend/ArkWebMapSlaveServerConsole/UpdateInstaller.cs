using ArkBridgeSharedEntities.Entities;
using ArkBridgeSharedEntities.Entities.RemoteConfig;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;

namespace ArkWebMapSlaveServerConsole
{
    public class UpdateInstaller
    {
        public static void InstallUpdate(RemoteConfigFile_Release release)
        {
            //Determine if this is a Windows or Linux machine.
            bool isWindows = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows);
            RemoteConfigFile_Release_Binary binary;
            if (isWindows)
                binary = release.binaries["windows"];
            else
                binary = release.binaries["linux"];

            //Find a temporary installation location.
            string tempDir = Path.GetTempPath().TrimEnd('\\').TrimEnd('/')+"/arkwebmap_installer/";
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
            Directory.CreateDirectory(tempDir);

            //Download
            Console.Write("Downloading helper...");
            byte[] updater_binary;
            try
            {
                using (WebClient wc = new WebClient())
                    updater_binary = wc.DownloadData(binary.updater_url);
            } catch
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("Failed. Press enter to exit.");
                Console.ReadLine();
                return;
            }
            Console.Write("Done.");

            //Unzip
            Console.Write("\nUnzipping...");
            using(MemoryStream ms = new MemoryStream(updater_binary))
            {
                using(ZipArchive za = new ZipArchive(ms, ZipArchiveMode.Read))
                {
                    Directory.CreateDirectory(tempDir + "installer/");
                    za.ExtractToDirectory(tempDir + "installer/");
                }
            }
            Console.Write("Done.");

            //Write config
            Console.WriteLine("\nWriting config...");
            UpdaterConfig config = new UpdaterConfig
            {
                binary_url = binary.url,
                output_path = System.AppDomain.CurrentDomain.BaseDirectory.TrimEnd('/').TrimEnd('\\')+"/",
                source_version = 1,
                exe_name = binary.exe_name
            };
            File.WriteAllText(tempDir + "installer/installer_config.json", JsonConvert.SerializeObject(config));
            Console.Write("Done.");

            //Execute the helper
            Process p = Process.Start(new ProcessStartInfo
            {
                WorkingDirectory = tempDir + "installer/",
                FileName = tempDir + "installer/" + binary.updater_cmd,
                WindowStyle = ProcessWindowStyle.Hidden
            });

            //End ourselves.
        }
    }
}
