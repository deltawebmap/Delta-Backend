
using ArkWebMapMasterServer.NetEntities.UserDataDownloader;
using ArkWebMapMasterServer.Tools;
using LibDeltaSystem.Db.System;
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
        public static async Task OnCreateRequest(Microsoft.AspNetCore.Http.HttpContext e, DbUser u, string token)
        {
            //Create a zip entry
            MemoryStream ms = new MemoryStream();
            using (ZipArchive zip = new ZipArchive(ms, ZipArchiveMode.Create, true))
            {
                //Add ARK user
                WriteJsonToZip(zip, "me.json", u);

                //Get servers this user owns
                WriteJsonToZip(zip, "owned_servers.json", u.GetOwnedServersAsync(Program.connection).GetAwaiter().GetResult());

                //Download remote data
                Task[] remote = new Task[] {
                        WriteAnalyticsZip(zip, token)
                    };

                //Wait for remote data
                Task.WaitAll(remote);
            }

            //Put file
            string url = TokenFileDownloadTool.PutFile(ms, "delta_user_archive.zip");

            //Generate a response
            await Program.QuickWriteJsonToDoc(e, new ArchiveCreateToken
            {
                ok = true,
                url = url
            });
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
