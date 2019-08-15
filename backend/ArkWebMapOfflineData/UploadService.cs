using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ArkWebMapGatewayClient.Messages;
using ArkWebMapOfflineData.Entities;
using LibDelta;

namespace ArkWebMapOfflineData
{
    public static class UploadService
    {
        public static async Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e)
        {
            //Authenticate this server
            var auth = await DeltaAuth.AuthenticateServer(e.Request.Headers["X-Ark-Slave-Server-ID"], e.Request.Headers["X-Ark-Slave-Server-Creds"]);

            //Check if failed
            if(auth == null)
            {
                await WebServerTools.QuickWriteToDoc(e, "Server Auth Failed", "text/plain", 401);
                return;
            }

            //Generate an ID for this
            var commits = Program.GetCommitCollection();
            string commitTag = WebServerTools.GenerateRandomString(24);
            while (commits.FindById(commitTag) != null)
                commitTag = WebServerTools.GenerateRandomString(24);

            //Create commit
            DataCommit commit = new DataCommit
            {
                data_version = -1,
                files = new Dictionary<int, string>(),
                is_ready = false,
                time = DateTime.UtcNow.Ticks,
                _id = commitTag
            };
            commits.Insert(commit);

            //Accept and begin reading stream
            //Since offline reports can be large, they follow a special format. For integer size, read the integer tribe ID, integer length, and then GZipped compressed data
            int version;
            try
            {
                version = await ReadIntFromStream(e.Request.Body);
                int arraySize = await ReadIntFromStream(e.Request.Body);
                for (int i = 0; i < arraySize; i++)
                {
                    //Read header
                    int tribeId = await ReadIntFromStream(e.Request.Body);
                    int contentLength = await ReadIntFromStream(e.Request.Body);

                    //Write to LiteDB file. Generate an ID
                    string fileTag = WebServerTools.GenerateRandomString(24);
                    while(Program.db.FileStorage.Exists(fileTag))
                        fileTag = WebServerTools.GenerateRandomString(24);

                    //Create file and write to it
                    using (Stream f = Program.db.FileStorage.OpenWrite(fileTag, fileTag))
                        await CopyStream(e.Request.Body, f, contentLength);

                    //Add to commit
                    commit.files.Add(tribeId, fileTag);
                }
            }
            catch (Exception ex)
            {
                await WebServerTools.QuickWriteToDoc(e, "Read Failure", "text/plain", 500);
                return;
            }

            //Update and save commit
            commit.data_version = version;
            commit.is_ready = true;
            commits.Update(commit);

            //Get this server (if any) and update it
            var servers = Program.GetServerCollection();
            DataServer s = servers.FindById(auth.server_id);
            bool exists = s != null;
            if (s == null)
                s = new DataServer();
            s._id = auth.server_id;
            string oldCommit = s.previous_commit;
            s.previous_commit = s.latest_commit;
            s.latest_commit = commitTag;
            if (exists)
                servers.Update(s);
            else
                servers.Insert(s);

            //Now, if there was a previous commit, we'll delete it
            if (oldCommit != null)
                await DeleteCommit(oldCommit);

            //Send message on gateway
            DeltaMapTools.gateway.SendMessage(new MessageSubserverOfflineDataUpdated
            {
                data_version = version,
                server_id = auth.server_id,
                headers = new Dictionary<string, string>(),
                opcode = ArkWebMapGatewayClient.GatewayMessageOpcode.SubserverOfflineDataUpdated
            });

            //Now, return the commit ID
            await WebServerTools.QuickWriteToDoc(e, commitTag, "text/plain", 200);
        }

        public static async Task DeleteCommit(string tag)
        {
            //Get the commit
            var collec = Program.GetCommitCollection();
            var commit = collec.FindById(tag);
            if (commit == null)
                return;

            //Now, delete all files associated
            foreach (string i in commit.files.Values)
                Program.db.FileStorage.Delete(i);

            //Delete the commit
            collec.Delete(commit._id);
        }

        private static async Task CopyStream(Stream input, Stream output, int size)
        {
            byte[] buf = new byte[2048];
            int read = 0;
            while(read<size)
            {
                int s = Math.Min(size - read, 2048);
                await input.ReadAsync(buf, 0, s);
                await output.WriteAsync(buf, 0, s);
                read += s;
            }
        }

        private static async Task<int> ReadIntFromStream(Stream s)
        {
            byte[] buf = new byte[4];
            await s.ReadAsync(buf, 0, buf.Length);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(buf);
            return BitConverter.ToInt32(buf);
        }
    }
}
