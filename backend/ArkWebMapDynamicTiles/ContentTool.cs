using ArkBridgeSharedEntities.Requests;
using ArkWebMapDynamicTiles.Entities;
using LiteDB;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace ArkWebMapDynamicTiles
{
    /// <summary>
    /// Manages content
    /// </summary>
    public static class ContentTool
    {
        public static LiteDatabase db;

        public static T GetContent<T>(string key)
        {
            //Get temp stream to decompress to
            T output;
            using (Stream ms = db.FileStorage.OpenRead(key))
            {
                byte[] buf = new byte[ms.Length];
                ms.Read(buf, 0, buf.Length);
                output = JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(buf));
            }
            return output;
        }

        public static ContentMetadata GetCommit(string serverId)
        {
            return db.GetCollection<ContentMetadata>("content").FindById(serverId);
        }

        //Uploader

        /// <summary>
        /// Uploads content and returns the upload token.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string PutContentGetToken(Stream s)
        {
            //Generate a unique token
            string token = Program.GenerateRandomString(32);
            while (db.FileStorage.Exists(token))
                token = Program.GenerateRandomString(32);

            //Create
            db.FileStorage.Upload(token, token, s);

            return token;
        }

        public static void CommitContent(DynamicTileContentPost request)
        {
            //Get the collection.
            var collec = db.GetCollection<ContentMetadata>("content");

            //Create content object
            ContentMetadata content = new ContentMetadata
            {
                content = request.tokens,
                time = DateTime.UtcNow.Ticks,
                version = request.version,
                _id = request.server_id,
                revision = Program.GenerateRandomString(24)
            };

            //Create or update
            ContentMetadata oldData = collec.FindById(request.server_id);
            if (oldData == null)
                collec.Insert(content);
            else
                collec.Update(content);

            //If old data exists, clear it
            if(oldData != null)
            {
                foreach(var o in oldData.content.Values)
                {
                    db.FileStorage.Delete(o);
                }
            }
        }
    }
}
