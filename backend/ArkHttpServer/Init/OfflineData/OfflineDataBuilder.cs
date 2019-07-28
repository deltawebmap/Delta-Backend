using ArkBridgeSharedEntities.Entities;
using ArkHttpServer.Entities;
using ArkHttpServer.HttpServices;
using ArkSaveEditor.World;
using ArkSaveEditor.World.WorldTypes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text;

namespace ArkHttpServer.Init.OfflineData
{
    public static class OfflineDataBuilder
    {
        public static bool SendOfflineData()
        {
            //Log
            Console.WriteLine("Building offline data...");

            //Set last report
            ArkWebServer.lastOfflineReport = DateTime.UtcNow;

            //Get world
            ArkWorld w = WorldLoader.GetWorld(out DateTime time);

            //Create the stream we will send, then encode using our special format
            using (MemoryStream s = new MemoryStream())
            {
                WriteInt32ToStream(s, ClientVersion.DATA_VERSION); //Version
                WriteInt32ToStream(s, w.tribes.Count); //Number of tribes
                for (int i = 0; i < w.tribes.Count; i++)
                {
                    //Generate offline tribe
                    ArkTribeProfile t = w.tribes[i];
                    ArkSlaveReport_OfflineTribe ot = GenerateOverviewForSingleTribe(w, t.tribeId, time);
                    byte[] payload = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(ot));

                    //Write tribe ID
                    WriteInt32ToStream(s, t.tribeId);

                    //Compress payload
                    using (MemoryStream compressed = new MemoryStream())
                    {
                        using (GZipStream gz = new GZipStream(compressed, CompressionLevel.Optimal, true))
                            gz.Write(payload, 0, payload.Length);

                        //Write the length of the compressed stream
                        WriteInt32ToStream(s, (int)compressed.Length);

                        //Copy compressed data to stream
                        compressed.Position = 0;
                        compressed.CopyTo(s);
                    }
                }

                //Rewind and send over network
                s.Position = 0;
                Console.WriteLine($"Sending offline data ({MathF.Round(s.Length / 1024)} KB)...");
                MasterServerSender.SendRequestToMasterGetBytes("offline_data_report", new StreamContent(s));
            }

            return true;
        }

        static void WriteInt32ToStream(Stream s, int i)
        {
            byte[] buf = BitConverter.GetBytes(i);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(buf);
            s.Write(buf, 0, 4);
        }

        static ArkSlaveReport_OfflineTribe GenerateOverviewForSingleTribe(ArkWorld w, int tribeId, DateTime lastSavedAtTime)
        {
            ArkSlaveReport_OfflineTribe result = new ArkSlaveReport_OfflineTribe();
            result.overview = TribeOverviewService.GenerateReply(w, tribeId);
            result.tribe = new BasicTribe(w, tribeId);
            result.session = new BasicArkWorld(w, lastSavedAtTime);
            result.version = ClientVersion.DATA_VERSION;
            return result;
        }
    }
}
