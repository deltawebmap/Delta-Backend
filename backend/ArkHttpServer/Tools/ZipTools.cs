using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using Newtonsoft.Json;

namespace ArkHttpServer.Tools
{
    public static class ZipTools
    {
        public static T ReadEntryAsJson<T>(string name, ZipArchive z)
        {
            var entry = z.GetEntry(name);
            byte[] buf = new byte[entry.Length];
            using(Stream s = entry.Open())
            {
                s.Read(buf, 0, buf.Length);
            }
            return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(buf));
        }
    }
}
