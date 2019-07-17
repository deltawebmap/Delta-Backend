using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.IO.Compression;
using Newtonsoft.Json;

namespace ArkWebMapLauncherProviders
{
    public static class UpdaterTool
    {
        public static bool isQuittingForUpdate = false;

        public static bool Update(LauncherConfig config, out PackageInfo pi)
        {
            Console.WriteLine("Current subserver is out of date! Downloading update...");
            pi = null;

            //Get the platform binary
            LauncherConfig_SubserverRelease release = config.latest_subserver;
            LauncherConfig_SubserverReleaseBinary binary = Program.GetPlatformBinary(release);

            //Now, download the zip file for this
            byte[] binaryRelease;
            try
            {
                using (WebClient wc = new WebClient())
                    binaryRelease = wc.DownloadData(binary.url);
            } catch (Exception ex)
            {
                Program.ExitWithFatalError("Failed to download update with error " + ex.Message);
                return false;
            }

            Console.WriteLine("Download finished. Unpacking...");

            //Remove the latest release directory if it exists
            try
            {
                if (Directory.Exists(Program.GetRelPath("latest_subserver/")))
                    Directory.Delete(Program.GetRelPath("latest_subserver/"), true);
            } catch (Exception ex)
            {
                Program.ExitWithFatalError("Failed to delete existing subserver install with error " + ex.Message);
                return false;
            }

            //Unpack the ZIP files
            string rootDir = Program.GetRelPath("latest_subserver/");
            try
            {
                //Create directory
                Directory.CreateDirectory(rootDir);

                //Unpack ZIP
                using(MemoryStream ms = new MemoryStream(binaryRelease))
                {
                    using (ZipArchive za = new ZipArchive(ms, ZipArchiveMode.Read))
                    {
                        za.ExtractToDirectory(rootDir);
                    }
                }
            } catch (Exception ex)
            {
                Program.ExitWithFatalError("Failed to unpack with error " + ex.Message);
                return false;
            }

            //Verify and finish
            Console.WriteLine("Verifying...");

            if(!File.Exists(rootDir+binary.binary_pathname))
            {
                Program.ExitWithFatalError("Verification failed because the binary executable did not exist. Bad release? Try again later.");
                return false;
            }

            //Write the packageinfo
            pi = new PackageInfo
            {
                binary_pathname = binary.binary_pathname,
                version = release.version,
                installedAt = DateTime.UtcNow
            };
            File.WriteAllText(Program.GetRelPath("subserver_packageinfo.json"), JsonConvert.SerializeObject(pi));

            //OK!
            Console.WriteLine("Update finished!");
            return true;
        }
    }
}
