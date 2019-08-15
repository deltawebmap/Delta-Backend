using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArkWebMapOfflineData.Entities;
using LibDelta;
using Newtonsoft.Json;

namespace ArkWebMapOfflineData
{
    public static class DownloadService
    {
        public static async Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e)
        {
            //Get the server ID from the URL
            string id = e.Request.Path.ToString().Substring("/content/".Length);

            //Authenticate this user
            if(!e.Request.Headers.ContainsKey("Authorization") || e.Request.Headers["Authorization"].ToString().Length < "Bearer ".Length)
            {
                await WebServerTools.QuickWriteToDoc(e, "No Token Provided", "text/plain", 401);
                return;
            }
            var user = await DeltaAuth.AuthenticateUser(e.Request.Headers["Authorization"].ToString().Substring("Bearer ".Length));
            if(user == null)
            {
                await WebServerTools.QuickWriteToDoc(e, "Not Authenticated", "text/plain", 401);
                return;
            }

            //Make sure this user has this server
            if(user.servers.Where( x => x.id == id ).Count() != 1)
            {
                await WebServerTools.QuickWriteToDoc(e, "Not A Member Of This Server", "text/plain", 403);
                return;
            }
            var tribe = user.servers.Where(x => x.id == id).First();

            //Now, we're going to get the server
            DataServer server = Program.GetServerCollection().FindById(id);
            if(server == null)
            {
                await WebServerTools.QuickWriteToDoc(e, "Server Has Not Uploaded Data", "text/plain", 404);
                return;
            }

            //Get the latest commit
            DataCommit commit = Program.GetCommitCollection().FindById(server.latest_commit);
            if (commit == null)
            {
                await WebServerTools.QuickWriteToDoc(e, "Server Has Not Uploaded Data", "text/plain", 404);
                return;
            }

            //Now get this tribe's file
            if(!commit.files.ContainsKey(tribe.tribeId))
            {
                await WebServerTools.QuickWriteToDoc(e, "Tribe ID Does Not Exist In Commit", "text/plain", 404);
                return;
            }

            //Set some headers
            e.Response.Headers.Add("X-Delta-Commit-ID", commit._id);
            e.Response.Headers.Add("X-Delta-Commit-Version", commit.data_version.ToString());
            e.Response.Headers.Add("X-Delta-Commit-Time-Unix", ((long)(new DateTime(commit.time).ToUniversalTime() - new DateTime(1970, 1, 1)).TotalSeconds).ToString());
            e.Response.Headers.Add("X-Delta-Commit-Time", JsonConvert.SerializeObject(new DateTime(commit.time)).Trim('"'));
            e.Response.Headers.Add("X-Delta-Commit-Previous", server.previous_commit);
            e.Response.Headers.Add("X-Delta-Server-ID", server._id);

            //Copy and decompress to the body
            using (Stream s = Program.db.FileStorage.OpenRead(commit.files[tribe.tribeId]))
            {
                e.Response.ContentType = "application/json";
                try
                {
                    using (GZipStream c = new GZipStream(s, CompressionMode.Decompress))
                        await c.CopyToAsync(e.Response.Body);
                } catch (Exception ex)
                {
                    if(!e.Response.HasStarted)
                    {
                        //There's still time to send a correct error message
                        await WebServerTools.QuickWriteToDoc(e, "Decompression Failure", "text/plain", 500);
                        return;
                    } else
                    {
                        //It's too late to correctly close this. Abort the connection.
                        e.Abort();
                    }
                }
            }
        }
    }
}
