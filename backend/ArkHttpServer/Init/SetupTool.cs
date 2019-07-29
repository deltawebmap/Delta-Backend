﻿using ArkBridgeSharedEntities.Entities.RemoteConfig;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace ArkHttpServer.Init
{
    public static class SetupTool
    {
        public static bool StartSetup(LaunchOptions options, RemoteConfigFile config)
        {
            //Obtain a code
            string code = WriteCode();
            if (code == null)
                return false;

            //Send the hello message
            bool ok = SendMessage(config, code, new ArkSetupProxyMessage
            {
                data = new Dictionary<string, string>
                    {
                        {"setup_version", StartupTool.CURRENT_RELEASE_ID.ToString() }
                    },
                type = ArkSetupProxyMessage_Type.ServerHello
            });
            if (ok == false)
                return false;

            //Go into the event loop
            while (true)
            {
                List<ArkSetupProxyMessage> messages = GetMessages(config, code);
                if (messages == null)
                    return false;
                foreach (ArkSetupProxyMessage message in messages)
                {
                    Console.WriteLine(JsonConvert.SerializeObject(message));
                    switch (message.type)
                    {
                        case ArkSetupProxyMessage_Type.CheckArkFile:
                            SetupMapFileTester.OnBeginRequest(options, config, code, message);
                            break;
                        case ArkSetupProxyMessage_Type.UploadConfigAndFinish:
                            //Deserialize and save
                            File.WriteAllText(options.path_config, message.data["config"]);

                            //Respond
                            return SendMessage(config, code, new ArkSetupProxyMessage
                            {
                                data = new Dictionary<string, string>(),
                                type = ArkSetupProxyMessage_Type.ServerGoodbye
                            });
                    }
                }
                Thread.Sleep(1000);
            }
        }

        /// <summary>
        /// Obtains a code and tells the user to enter it
        /// </summary>
        /// <returns></returns>
        private static string WriteCode()
        {
            //Obtain a code
            string code;
            try
            {
                var request = (HttpWebRequest)WebRequest.Create("https://deltamap.net/api/obtain_server_setup_proxy_code");
                request.Method = "GET";
                var response = (HttpWebResponse)request.GetResponse();
                string responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                SetupServerProxy_ObtainCode c = JsonConvert.DeserializeObject<SetupServerProxy_ObtainCode>(responseString);
                code = c.code;
            }
            catch (System.Net.WebException ex)
            {
                Console.WriteLine("Sorry, could not obtain setup code. Try again later. Error code -2.");
                return null;
            }

            //Now, write it out
            Console.WriteLine("Welcome to Delta Web Map! Here is your setup code.");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(code);
            Console.ForegroundColor = ConsoleColor.White;
            return code;
        }

        class SetupServerProxy_ObtainCode
        {
            public string code;
        }

        public static bool SendMessage(RemoteConfigFile conf, string code, ArkSetupProxyMessage msg)
        {
            string url = GetCommUrl(conf, code);
            string ser_string = JsonConvert.SerializeObject(msg);
            byte[] ser = Encoding.UTF8.GetBytes(ser_string);
            var request = (HttpWebRequest)WebRequest.Create(url);

            request.Method = "POST";
            request.ContentLength = ser.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(ser, 0, ser.Length);
            }

            var response = (HttpWebResponse)request.GetResponse();

            if (response.StatusCode != HttpStatusCode.OK)
            {
                Console.WriteLine("Failed to send message to setup. Error code -3.");
                return false;
            }
            return true;
        }

        private static List<ArkSetupProxyMessage> GetMessages(RemoteConfigFile conf, string code)
        {
            string url = GetCommUrl(conf, code);
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                var response = (HttpWebResponse)request.GetResponse();
                string responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                return JsonConvert.DeserializeObject<List<ArkSetupProxyMessage>>(responseString);
            }
            catch
            {
                Console.WriteLine("Failed to get messages from the server during setup. Error code -4.");
                return null;
            }
        }

        private static string GetCommUrl(RemoteConfigFile conf, string code)
        {
            return conf.sub_server_config.endpoints.server_setup_proxy.Replace("{clientSessionId}", code);
        }
    }
}