using ArkBridgeSharedEntities.Entities;
using ArkSaveEditor.World;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ArkWebMapSlaveServerConsole
{
    public class MapFileTester
    {
        public static void OnBeginRequest(ArkSetupProxyMessage message)
        {
            //Check and open ARK file
            string path = message.data["path"];

            //Check if file exists
            if(File.Exists(path))
            {
                //Try to open this as an ARK file
                try
                {
                    var map = ArkSaveEditor.Deserializer.ArkSaveDeserializer.OpenDotArk(path);
                    Respond(true, map != null);
                } catch (Exception ex)
                {
                    Respond(true, false);
                }
            } else
            {
                //Does not even exist.
                Respond(false, false);
            }
        }

        static void Respond(bool fileExists, bool isValidArk)
        {
            Program.SendMasterMessage(new ArkSetupProxyMessage
            {
                type = ArkSetupProxyMessage_Type.CheckArkFileResponse,
                data = new Dictionary<string, string>
                {
                    {"exists", fileExists.ToString().ToLower() },
                    {"isValidArk", isValidArk.ToString().ToLower() }
                }
            });
        }
    }
}
