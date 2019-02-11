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
            //Quietly open the config file
            UpdaterConfig config;
            try
            {
                config = JsonConvert.DeserializeObject<UpdaterConfig>(File.ReadAllText("installer_config.json"));
            } catch
            {
                FailWrite("Failed to open installer config.");
                return;
            }

            //Download the update package.
            StartStepWrite("Downloading update package...");
            byte[] updater_binary;
            try
            {
                using (WebClient wc = new WebClient())
                    updater_binary = wc.DownloadData(config.binary_url);
            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("Failed. Press enter to exit.");
                Console.ReadLine();
                return;
            }
            Console.Write("Done.");

            //Unzip
            Console.Write("\nUnzipping...");
            using (MemoryStream ms = new MemoryStream(updater_binary))
            {
                using (ZipArchive za = new ZipArchive(ms, ZipArchiveMode.Read))
                {
                    za.ExtractToDirectory(config.output_path, true);
                }
            }
            Console.Write("Done.");
            Console.Clear();

            //Restart program
            Process.Start(new ProcessStartInfo
            {
                WorkingDirectory = config.output_path,
                FileName = config.output_path + config.exe_name
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
