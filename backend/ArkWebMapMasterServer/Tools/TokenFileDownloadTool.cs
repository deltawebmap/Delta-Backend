
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace ArkWebMapMasterServer.Tools
{
    /// <summary>
    /// Allows generation of one-time downloads with multiple requests
    /// </summary>
    public static class TokenFileDownloadTool
    {
        private static Dictionary<string, SetFile> files = new Dictionary<string, SetFile>();
        private static Timer expireTimer;

        public static void Init()
        {
            expireTimer = new Timer(TimeSpan.FromMinutes(1).TotalMilliseconds);
            expireTimer.AutoReset = true;
            expireTimer.Elapsed += ExpireTimer_Elapsed;
            expireTimer.Start();
        }

        private static void ExpireTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            //Find exired sessions
            lock(files)
            {
                List<string> expiredKeys = new List<string>();
                foreach (var f in files)
                {
                    if (f.Value.expire <= DateTime.UtcNow)
                        expiredKeys.Add(f.Key);
                }
                foreach (var f in expiredKeys)
                {
                    files[f].file.Close();
                    files.Remove(f);
                }
            }
        }

        public static string PutFile(Stream s, string name, TimeSpan? expireOffset = null)
        {
            //Get expire time
            DateTime expire = DateTime.UtcNow;
            if (!expireOffset.HasValue)
                expire = expire.AddMinutes(15);
            else
                expire = expire + expireOffset.Value;

            string code;
            lock(files)
            {
                //Generate a code
                code = Program.GenerateRandomStringCustom(32, "ABCDEF1234567890".ToCharArray());
                while (files.ContainsKey(code))
                    code = Program.GenerateRandomStringCustom(32, "ABCDEF1234567890".ToCharArray());

                //Create entry
                files.Add(code, new SetFile
                {
                    expire = expire,
                    file = s,
                    name = name
                });
            }

            //Return download URL
            return "https://deltamap.net/api/download_token?token="+code;
        }

        public static async Task OnDownloadRequest(Microsoft.AspNetCore.Http.HttpContext e)
        {
            //We'll actually download the archive. Get the stream
            string token = e.Request.Query["token"];
            if (!files.ContainsKey(token))
                throw new StandardError("Token invalid", StandardErrorCode.AuthFailed);

            //Copy stream
            SetFile file = files[token];
            Stream ms = file.file;
            ms.Position = 0;
            e.Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{file.name}\"");
            e.Response.ContentType = "application/octet-stream";
            e.Response.ContentLength = ms.Length;
            await ms.CopyToAsync(e.Response.Body);
        }

        class SetFile
        {
            public DateTime expire;
            public string name;
            public Stream file;
        }
    }
}
