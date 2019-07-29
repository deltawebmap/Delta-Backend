using ArkHttpServer.Init;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkHttpServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //Log
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"DeltaWebMap for Ark - Version {ClientVersion.VERSION_MAJOR}.{ClientVersion.VERSION_MINOR} (data version {ClientVersion.DATA_VERSION})");
            Console.ForegroundColor = ConsoleColor.White;

            //Try and launch
            bool ok = StartupTool.OnStartup(args);

            //Check if we failed
            if(!ok)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Launch failed. Press enter to quit.");
                Console.ReadLine();
                return;
            }

            //Wait forever
            Console.WriteLine("Ready!");
            Task.Delay(-1).GetAwaiter().GetResult();
        }
    }
}
