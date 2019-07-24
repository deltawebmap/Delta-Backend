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

                //Log
                if(data != null)
                {
                    Console.WriteLine("Submitted mod token " + data.token + " with " + data.dinoTokenMap.Keys.Count + " map entries.");
                } else
                {
                    Console.WriteLine("No mod token found.");
                }

                //Send to the master server
                MasterServerSender.SendRequestToMaster("mirror_report", data, typeof(MirrorReportResponse));
            } catch (Exception ex)
            {
                LogError("Unknown fatal error; "+ex.Message+ex.StackTrace);
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
            var refsTokensRaw = data.GetPropByName("dinoRefsToken");
            var refsToken = (ArrayProperty<string>)refsTokensRaw;

            //Put it into our own data class
            ArkWebMapMirrorTokens output = new ArkWebMapMirrorTokens
            {
                token = token,
                dinoTokenMap = new Dictionary<string, string>(),
                updateTime = DateTime.UtcNow.Ticks
            };
            if (refs.items.Count != refsToken.items.Count)
            {
                LogError("Dino token length mismatch.");
                return null;
            }
            for(int i = 0; i<refs.items.Count; i++)
            {
                DotArkGameObject r = refs.items[i].gameObjectRef;
                if(r != null)
                {
                    string t = refsToken.items[i];
                    string id = GetDinoId(r);
                    output.dinoTokenMap.Add(t, id);
                }
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
