using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

namespace ArkWebMapLauncher
{
    public static class BackgroundWorker
    {
        private static PackageInfo latestPackageInfo;
        private static Timer bgWorker;

        public static void StartBackgroundWorker(LauncherConfig config, PackageInfo pinfo)
        {
            bgWorker = new Timer();
            bgWorker.Elapsed += OnBackgroundWorkerTick;
            bgWorker.Interval = config.launcher_options.bg_refresh_ms;
            bgWorker.AutoReset = true;
            latestPackageInfo = pinfo;
            bgWorker.Start();
        }

        public static void StopBackgroundWorker()
        {
            bgWorker.Stop();
        }

        private static void OnBackgroundWorkerTick(object sender, ElapsedEventArgs e)
        {
            //Check for updates
            LauncherConfig config;
            try
            {
                config = Program.DownloadClass<LauncherConfig>($"https://ark.romanport.com/launcher_config.json?is_windows={Program.GetIsWindows().ToString()}&launcher_version={Program.LAUNCHER_VERSION}&launcher_type=BLUE&is_bg=true");
                OnBackgroundWorkerTickCheckConfig(config);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to check update status in background. Ignoring, will try again later...");
            }
        }

        private static void OnBackgroundWorkerTickCheckConfig(LauncherConfig updatedConfig)
        {
            //If the current package is out of date, update
            if (latestPackageInfo.version < updatedConfig.latest_subserver.version)
            {
                //Suspend updates
                StopBackgroundWorker();

                //Write to log
                Console.WriteLine("An update was just released. Service will be interrupted temporarily while the update is downloaded.");

                //Stop current process, if any
                UpdaterTool.isQuittingForUpdate = true;
                lock (Program.mainProcess)
                {
                    try
                    {
                        Program.mainProcess.CloseMainWindow();
                        System.Threading.Thread.Sleep(1000);
                    }
                    catch(Exception ex) { Console.WriteLine(ex.Message); }
                    try
                    {
                        Program.mainProcess.Kill();
                        System.Threading.Thread.Sleep(1000);
                    }
                    catch (Exception ex) { Console.WriteLine(ex.Message); }
                }

                //Update
                UpdaterTool.Update(updatedConfig, out latestPackageInfo);

                //Write to log
                Console.WriteLine("Updated. Restarting service now...");

                //Start service back up
                Program.StartMainService(latestPackageInfo);
                UpdaterTool.isQuittingForUpdate = false;

                //Continue updates
                StartBackgroundWorker(updatedConfig, latestPackageInfo);
            }
        }
    }
}
