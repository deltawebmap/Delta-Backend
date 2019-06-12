using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace ArkWebMapSlaveServerConsole
{
    partial class Program
    {
        /// <summary>
        /// A bit ugly. Gives the user the status code.
        /// </summary>
        static string IntroAndPromptUserForCode()
        {
            while(true)
            {
                Console.Clear();
                DrawWithColor("Welcome!", ConsoleColor.Cyan);
                DrawWithColor(" Welcome to the ArkWebMap companion software.\n\nFetching code...");
                try
                {
                    var request = (HttpWebRequest)WebRequest.Create("https://ark.romanport.com/api/obtain_server_setup_proxy_code");
                    request.Method = "GET";
                    var response = (HttpWebResponse)request.GetResponse();
                    string responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                    SetupServerProxy_ObtainCode code = JsonConvert.DeserializeObject<SetupServerProxy_ObtainCode>(responseString);
                    DrawWithColor("\rType the code into the ArkWebMap setup page: ");
                    DrawWithColor(code.code + "        ", ConsoleColor.Blue);
                    return code.code;
                }
                catch (System.Net.WebException ex)
                {
                    DrawWithColor("\rCould not connect. Are you offline? Press enter to try again.        ", ConsoleColor.Red);
                    Console.ReadLine();
                }
            }
        }
    }

    class SetupServerProxy_ObtainCode
    {
        public string code;
    }
}
