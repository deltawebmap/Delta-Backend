using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ArkBridgeSharedEntities.Entities.RemoteConfig;

namespace ArkHttpServer.Init
{
    static class SetupMapFileTester
    {
        public static void OnBeginRequest(LaunchOptions options, RemoteConfigFile config, string code, ArkSetupProxyMessage message)
        {
            //Check and open ARK file
            string path = message.data["path"];
            string rid = message.data["rid"];

            //Check if file exists
            if (File.Exists(path))
            {
                //Try to open this as an ARK file
                try
                {
                    var map = ArkSaveEditor.Deserializer.ArkSaveDeserializer.OpenDotArk(path);
                    Respond(options, config, code, true, map != null, rid);
                }
                catch (Exception ex)
                {
                    Respond(options, config, code, true, false, rid);
                }
            }
            else
            {
                //Does not even exist.
                Respond(options, config, code, false, false, rid);
            }
        }

        static void Respond(LaunchOptions options, RemoteConfigFile config, string code, bool fileExists, bool isValidArk, string rid)
        {
            SetupTool.SendMessage(config, code, new ArkSetupProxyMessage
            {
                type = ArkSetupProxyMessage_Type.CheckArkFileResponse,
                data = new Dictionary<string, string>
                {
                    {"exists", fileExists.ToString().ToLower() },
                    {"isValidArk", isValidArk.ToString().ToLower() },
                    {"rid",rid }
                }
            });
        }
    }
}
