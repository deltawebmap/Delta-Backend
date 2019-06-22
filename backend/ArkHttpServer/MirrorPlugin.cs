using ArkHttpServer.Entities;
using ArkSaveEditor.Entities.LowLevel.DotArk;
using ArkSaveEditor.Entities.LowLevel.DotArk.ArkProperties;
using ArkSaveEditor.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArkHttpServer
{
    public static class MirrorPlugin
    {
        public static void OnMapSave(ArkWorld world)
        {
            try
            {
                //Find the plugin class, if it exists
                ArkWebMapMirrorTokens data = GetArkWebMapTokens(world);

                //Send to the master server
                ArkWebServer.sendRequestToMasterCode("mirror_report", data, typeof(MirrorReportResponse));
            } catch
            {
                LogError("Unknown fatal error.");
            }
        }

        private static ArkWebMapMirrorTokens GetArkWebMapTokens(ArkWorld world)
        {
            //Find the plugin class, if it exists, and get it's data
            var results = world.sources.Where(x => x.classname.classname == "ArkWebMapTokenObject_C");
            if(results.Count() > 1)
            {
                LogError("More than two entries exist in the file.");
                return null;
            }
            if (results.Count() < 1)
                return null;
            var data = results.First();

            //Now, gather info from inside of the class
            string token = (string)data.GetPropByName<StrProperty>("token").data;
            var refs = data.GetPropByName<ArrayProperty<ObjectProperty>>("dinoRefs");
            var refsTokens = data.GetPropByName<ArrayProperty<StrProperty>>("dinoRefsToken");

            //Put it into our own data class
            ArkWebMapMirrorTokens output = new ArkWebMapMirrorTokens
            {
                token = token,
                dinoTokenMap = new Dictionary<string, string>(),
                updateTime = DateTime.UtcNow.Ticks
            };
            if (refs.items.Count != refsTokens.items.Count)
            {
                LogError("Dino token length mismatch.");
                return null;
            }
            for(int i = 0; i<refs.items.Count; i++)
            {
                DotArkGameObject r = refs.items[i].gameObjectRef;
                string t = (string)refs.items[i].data;
                string id = GetDinoId(r);
                output.dinoTokenMap.Add(t, id);
            }

            //We did it!
            return output;
        }

        private static string GetDinoId(DotArkGameObject r)
        {
            //Read the dinosaur ID by combining the the bytes of the two UInt32 values.
            byte[] buf = new byte[8];
            BitConverter.GetBytes((UInt32)r.GetPropByName<UInt32Property>("DinoID1").data).CopyTo(buf, 0);
            BitConverter.GetBytes((UInt32)r.GetPropByName<UInt32Property>("DinoID2").data).CopyTo(buf, 4);

            //Convert this to a ulong
            ulong dinosaurId = BitConverter.ToUInt64(buf, 0);

            //Read to string
            return dinosaurId.ToString();
        }

        private static void LogError(string msg)
        {
            Console.WriteLine("Problem with Ark Mirror Plugin: " + msg);
        }

        class MirrorReportResponse
        {
            public bool ok;
        }
    }
}
