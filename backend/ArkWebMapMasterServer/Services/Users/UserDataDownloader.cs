using ArkBridgeSharedEntities.Entities;
using ArkWebMapMasterServer.NetEntities.UserDataDownloader;
using ArkWebMapMasterServer.PresistEntities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Users
{
    public static class UserDataDownloader
    {
        public static async Task OnCreateRequest(Microsoft.AspNetCore.Http.HttpContext e, ArkUser u, string token)
        {
            //Create a zip entry
            MemoryStream ms = new MemoryStream();
            using (ZipArchive zip = new ZipArchive(ms, ZipArchiveMode.Create, true))
            {
                //Add ARK user
                WriteJsonToZip(zip, "me.json", u);

                //Get servers this user owns
                WriteJsonToZip(zip, "owned_servers.json", ArkWebMapMasterServer.Servers.ArkSlaveServerSetup.GetCollection().Find(x => x.owner_uid == u._id).ToArray());

                //Get tokens and write them
                WriteJsonToZip(zip, "tokens.json", ArkWebMapMasterServer.Users.UserTokens.GetCollection().Find(x => x.uid == u._id).ToArray());

                //Download remote data
                Task[] remote = new Task[] {
                        WriteAnalyticsZip(zip, token)
                    };

                //Wait for remote data
                Task.WaitAll(remote);
            }

            //Create an entry in the stream tokens
            string stoken = Program.GenerateRandomString(24);
            while (stream_tokens.ContainsKey(stoken))
                stoken = Program.GenerateRandomString(24);
            stream_tokens.Add(stoken, ms);

            //Generate a response
            await Program.QuickWriteJsonToDoc(e, new ArchiveCreateToken
            {
                ok = true,
                url = "https://deltamap.net/api/archive_token?token=" + stoken
            });
        }

        public static Dictionary<string, Stream> stream_tokens = new Dictionary<string, Stream>();

        public static async Task OnDownloadRequest(Microsoft.AspNetCore.Http.HttpContext e)
        {
            //We'll actually download the archive. Get the stream
            string token = e.Request.Query["token"];
            if (!stream_tokens.ContainsKey(token))
                throw new StandardError("Token invalid", StandardErrorCode.AuthFailed);

            //Copy stream
            Stream ms = stream_tokens[token];
            ms.Position = 0;
            e.Response.ContentType = "application/zip";
            e.Response.ContentLength = ms.Length;
            await ms.CopyToAsync(e.Response.Body);
            ms.Close();

            //Now, delete stream
            stream_tokens.Remove(token);
        }

        static async Task WriteAnalyticsZip(ZipArchive zip, string token)
        {
            //Get
            HttpClient hc = new HttpClient();
            var stream = await hc.GetStreamAsync("https://web-analytics.deltamap.net/v1/archive?access_token=" + token);
            using (ZipArchive source = new ZipArchive(stream, ZipArchiveMode.Read, false))
            {
                //Copy entries
                int id = 0;
                var entry = source.GetEntry($"events_{id}.json");
                while (entry != null)
                {
                    //Copy
                    using (Stream s = zip.CreateEntry($"analytics/events_{id}.json").Open())
                    using (Stream ss = entry.Open())
                        await ss.CopyToAsync(s);

                    //Get next
                    id++;
                    entry = source.GetEntry($"events_{id}.json");
                }
            }
            hc.Dispose();
        }

        static void WriteJsonToZip(ZipArchive zip, string name, object data)
        {
            //Serialize
            byte[] d = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data, Formatting.Indented));

            //Write
            var entry = zip.CreateEntry(name);
            using (Stream s = entry.Open())
                s.Write(d, 0, d.Length);
        }
    }
}
