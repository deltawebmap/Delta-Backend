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
using System.Text.RegularExpressions;

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

            //Temp
            tempDir = @"E:\ArkWebMap\test_temp\";

            //Execute the helper
            string output = System.AppDomain.CurrentDomain.BaseDirectory.TrimEnd('/').TrimEnd('\\') + "/";
            Process p = Process.Start(new ProcessStartInfo
            {
                WorkingDirectory = tempDir + "installer/",
                FileName = tempDir + "installer/" + binary.updater_cmd,
                WindowStyle = ProcessWindowStyle.Hidden,
                Arguments = "\"" + Regex.Replace(output, @"(\\+)$", @"$1$1") + "\"" //Thanks to https://stackoverflow.com/questions/5510343/escape-command-line-arguments-in-c-sharp for helping with this insanity

            });

            //End ourselves.
        }
    }
}
