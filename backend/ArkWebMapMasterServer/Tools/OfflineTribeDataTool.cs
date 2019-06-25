using ArkWebMapMasterServer.PresistEntities;
using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace ArkWebMapMasterServer.Tools
{
    public static class OfflineTribeDataTool
    {
        public static LiteCollection<ArkServerOfflineData> GetCollection()
        {
            return Program.db.GetCollection<ArkServerOfflineData>("server_offline_data");
        }

        public static void UpdateArkData(string serverId, int tribeId, byte[] data)
        {
            //Create entry
            ArkServerOfflineData entry = new ArkServerOfflineData
            {
                content = data,
                time = DateTime.UtcNow.Ticks,
                _id = serverId + "/" + tribeId.ToString()
            };

            //Check if this is already in the database
            var collec = GetCollection();
            var existingResults = collec.FindById(serverId + "/" + tribeId.ToString());
            if (existingResults == null)
                collec.Insert(entry);
            else
                collec.Update(entry);
        }

        public static bool HasOfflineDataForTribe(string serverId, int tribeId)
        {
            var collec = GetCollection();
            return collec.FindById(serverId + "/" + tribeId.ToString()) != null; 
        }

        public static byte[] GetArkData(string serverId, int tribeId, out DateTime time)
        {
            //Grab from database
            var collec = GetCollection();
            var existingResults = collec.FindById(serverId + "/" + tribeId.ToString());
            time = DateTime.MinValue;
            if (existingResults == null)
                return null;
            time = new DateTime(existingResults.time);
            return existingResults.content;
        }

        public static bool GetArkDataDecompressedStreamed(string serverId, int tribeId, out DateTime time, Stream output)
        {
            byte[] data = GetArkData(serverId, tribeId, out time);
            if (data == null)
                return false;

            //This is a GZipped stream. Unzip it
            using (MemoryStream compressed = new MemoryStream(data))
            {
                using (GZipStream c = new GZipStream(compressed, CompressionMode.Decompress))
                    c.CopyTo(output);
            }
            return true;
        }

        public static string GetArkDataDecompressed(string serverId, int tribeId, out DateTime time)
        {
            byte[] buf;
            using (MemoryStream stream = new MemoryStream())
            {
                if (!GetArkDataDecompressedStreamed(serverId, tribeId, out time, stream))
                    return null;
                stream.Position = 0;
                buf = new byte[stream.Length];
                stream.Read(buf, 0, buf.Length);
            }
            return Encoding.UTF8.GetString(buf);
        }
    }
}
