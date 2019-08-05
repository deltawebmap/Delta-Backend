using ArkBridgeSharedEntities.Requests;
using ArkSaveEditor.World;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ArkHttpServer.Init
{
    public static class DynamicTileUploader
    {
        public const string ROOT_URL = "http://localhost:43295/";

        /// <summary>
        /// Uploads content, then returns an upload token that can be used.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static async Task<string> PostContent(object data)
        {
            //Serialize and compress
            using(MemoryStream ms = new MemoryStream())
            {
                //Compress
                /*using (GZipStream compressed = new GZipStream(ms, CompressionLevel.Optimal, true))
                {
                    byte[] ser = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data));
                    compressed.Write(ser, 0, ser.Length);
                }*/
                byte[] ser = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data));
                ms.Write(ser, 0, ser.Length);
                ms.Position = 0;

                //Send
                using(HttpClient hc = new HttpClient())
                {
                    var response = await hc.PostAsync(ROOT_URL+"upload", new StreamContent(ms));
                    if (!response.IsSuccessStatusCode)
                        throw new Exception("Failed.");
                    return await response.Content.ReadAsStringAsync();
                }
            }
        }

        /// <summary>
        /// Commits uploads
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static async Task Commit(DynamicTileContentPost content)
        {
            //Send
            using (HttpClient hc = new HttpClient())
            {
                var response = await hc.PostAsync(ROOT_URL + "commit", new StringContent(JsonConvert.SerializeObject(content)));
                if (!response.IsSuccessStatusCode)
                    throw new Exception("Failed.");
            }
        }

        /// <summary>
        /// Updates the data
        /// </summary>
        /// <param name="w"></param>
        /// <returns></returns>
        public static async Task UploadAll(ArkWorld w)
        {
            //Log
            Console.WriteLine("Uploading tiles data...");
            
            try
            {
                //Create the commit body
                DynamicTileContentPost commit = new DynamicTileContentPost
                {
                    server_creds = ArkWebServer.config.connection.creds,
                    server_id = ArkWebServer.config.connection.id,
                    tokens = new Dictionary<string, string>(),
                    version = ClientVersion.DATA_VERSION
                };

                //Upload content
                commit.tokens.Add("structures", await PostContent(w.structures));
                commit.tokens.Add("map", await PostContent(w.mapinfo));

                //Commit
                await Commit(commit);

                //End log
                Console.WriteLine("Tile data uploaded.");
            } catch
            {
                Console.WriteLine("Tile data upload failed.");
            }
        }
    }
}
