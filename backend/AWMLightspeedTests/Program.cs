using ArkWebMapLightspeedClient;
using System;
using System.Text;
using System.Threading.Tasks;

namespace AWMLightspeedTests
{
    class Program
    {
        static void Main(string[] args)
        {
            AWMLightspeedClient conn = AWMLightspeedClient.CreateClient("IRlrzrmxoe7M5rKiKrDD7zCf", "usoymPD4BNLTuTIDbSG1TcObolFSZUwCZdao3WWdL/sy1IxwHS97lUEVzE/V8eByAbjkWtWXMlR9LzMaUCdtNg==", 0, Handler, true);
            Console.ReadLine();
        }

        public static async Task Handler(LightspeedRequest req)
        {
            Console.WriteLine("Got request to " + req.endpoint);
            try
            {
                req.DoRespond(200, "text/plain", Encoding.UTF8.GetBytes("Hello World!"));
            } catch (Exception ex)
            {
                Console.WriteLine(ex.Message + ex.StackTrace);
            }
        }
    }
}
