using ArkWebMapAnalytics.NetEntities;
using ArkWebMapAnalytics.PersistEntities;
using LiteDB;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapAnalytics.Services
{
    public static class ArchiveService
    {
        public static async Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e)
        {
            //Authenticate
            string token = e.Request.Query["access_token"];
            string uid = UserAuthenticator.GetUserIdFromToken(token);

            //Check if failed
            if (uid == null)
                throw new StandardError("Must be authenticated.", 402);

            //Find all
            var data = Program.db.GetCollection<ActionEntry>("actions").Find( x => x.user_id == uid);

            //Add
            using(MemoryStream ms = new MemoryStream())
            {
                using(ZipArchive zip = new ZipArchive(ms, ZipArchiveMode.Create, true))
                {
                    //Add actions
                    List<ActionEntry> queue = new List<ActionEntry>();
                    int index = 0;
                    foreach(var a in data)
                    {
                        queue.Add(a);
                        if(queue.Count > 1000)
                        {
                            WriteEntryToZip(zip, $"events_{index}.json", queue);
                            queue.Clear();
                            index++;
                        }
                    }
                    WriteEntryToZip(zip, $"events_{index}.json", queue);
                    queue.Clear();

                    //Write metadata
                    WriteEntryToZip(zip, "metadata.json", new ArchiveMetadata
                    {
                        access_token = token,
                        time = DateTime.UtcNow,
                        user_id = uid,
                        version = 1
                    });
                }

                //Rewind and copy
                ms.Position = 0;
                e.Response.ContentType = "application/zip";
                e.Response.ContentLength = ms.Length;
                await ms.CopyToAsync(e.Response.Body);
            }
        }

        static void WriteEntryToZip(ZipArchive zip, string name, object data)
        {
            //Serialize
            byte[] d = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data));

            //Write
            var entry = zip.CreateEntry(name);
            using (Stream s = entry.Open())
                s.Write(d, 0, d.Length);
        }
    }
}
