using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace HookInExperiments
{
    class Program
    {
        public const int BUFFER_SIZE = 65536;

        static void Main(string[] args)
        {
            using(FileStream fs = new FileStream(@"C:\Program Files (x86)\Steam\steamapps\common\ARK\ShooterGame\Saved\Logs\ShooterGame.log", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                byte[] buf = new byte[100];
                fs.Read(buf, 0, buf.Length);
                Console.WriteLine(Encoding.UTF8.GetString(buf));
                Console.ReadLine();
            }
        }
    }
}
