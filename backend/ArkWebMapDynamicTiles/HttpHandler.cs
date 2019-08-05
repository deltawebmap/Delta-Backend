using ArkBridgeSharedEntities.Requests;
using ArkWebMapDynamicTiles.Entities;
using ArkWebMapDynamicTiles.Maps;
using ArkWebMapMasterServer.NetEntities;
using LibDelta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapDynamicTiles
{
    class HttpHandler
    {
        public const int GLOBAL_MIN_DATA_VERSION = 2;

        public static async Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e)
        {
            e.Response.Headers.Add("Server", "DeltaWebMap Dynamic Tiles");
            e.Response.Headers.Add("Access-Control-Allow-Headers", "Authorization");
            e.Response.Headers.Add("Access-Control-Allow-Origin", "https://deltamap.net");
            e.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS, DELETE, PUT, PATCH");

            //If this is an OPTIONS request, do CORS stuff
            if (e.Request.Method == "OPTIONS")
            {
                await Program.QuickWriteToDoc(e, "Hi, CORS!", "text/plain");
                return;
            }

            try
            {
                //Split the path. It follows this format: /{token}/{server id}/{map name}/{x}_{y}_{z}
                string[] split = e.Request.Path.ToString().Split('/');

                //Check if this is one of our special pathanmes
                if(e.Request.Path.ToString() == "/upload") { await OnContentPost(e); return; }
                if(e.Request.Path.ToString() == "/commit") { await OnCommitPost(e); return; }

                //Assume this is normal usage from now and check the path
                if (split.Length != 5)
                {
                    await Program.QuickWriteToDoc(e, "Invalid URL Structure", "text/plain", 400);
                    return;
                }
                string serverId = split[2];
                string mapType = split[3];
                string[] coords = split[4].Split('_');
                if (coords.Length != 3)
                {
                    await Program.QuickWriteToDoc(e, "Invalid URL Structure", "text/plain", 400);
                    return;
                }
                float x = float.Parse(coords[0]);
                float y = float.Parse(coords[1]);
                float z = float.Parse(coords[2]);

                //Authenticate this user
                UsersMeReply user = await AuthenticateUser(e, split);
                if (user == null)
                    return;

                //Ensure this user has this server on their list
                if(user.servers.Where(i => i.id == serverId).Count() != 1)
                {
                    await Program.QuickWriteToDoc(e, "You Are Not A Member Of This Server", "text/plain", 403);
                    return;
                }
                var server = user.servers.Where(i => i.id == serverId).First();

                //Load data for this server
                ContentMetadata commit = ContentTool.GetCommit(serverId);
                if(commit == null)
                {
                    await Program.QuickWriteToDoc(e, "This Server Has Not Uploaded Data Yet", "text/plain", 404);
                    return;
                }

                //Ensure this is an okay version
                if(commit.version < GLOBAL_MIN_DATA_VERSION)
                {
                    await Program.QuickWriteToDoc(e, "The Server Data Is Too Old", "text/plain", 404);
                    return;
                }

                //Switch off to the map to handle this
                if(mapType == "structures")
                {
                    await StructureTiles.OnHttpRequest(e, server, commit, x, y, z);
                    return;
                } else
                {
                    await Program.QuickWriteToDoc(e, "Unknown Map Type", "text/plain", 404);
                }
            } catch (Exception ex)
            {
                Console.WriteLine($"ERROR {ex.Message} {ex.StackTrace}");
                await Program.QuickWriteToDoc(e, "Internal Server Error", "text/plain", 500);
            }
        }

        private static async Task<UsersMeReply> AuthenticateUser(Microsoft.AspNetCore.Http.HttpContext e, string[] pathSplit)
        {
            //Find the token
            string token = null;

            //Check the headers first
            if (e.Request.Headers.ContainsKey("authorization"))
                token = e.Request.Headers["authorization"].ToString().Substring("Bearer ".Length);

            //Try the path where our token should be
            if (token == null)
                token = pathSplit[1];

            //Stop now if no token was found.
            if(token == null)
            {
                await Program.QuickWriteToDoc(e, "No Token Found", "text/plain", 401);
                return null;
            }

            //Try and authenticate
            UsersMeReply user = await DeltaAuth.AuthenticateUser(token);
            if(user == null)
            {
                //Not authenticated
                await Program.QuickWriteToDoc(e, "Not Authenticated", "text/plain", 401);
                return null;
            }
            return user;
        }

        private static async Task OnContentPost(Microsoft.AspNetCore.Http.HttpContext e)
        {
            //Create a content object and return the upload token
            string token = ContentTool.PutContentGetToken(e.Request.Body);
            await Program.QuickWriteToDoc(e, token, "text/plain");
        }

        private static async Task OnCommitPost(Microsoft.AspNetCore.Http.HttpContext e)
        {
            //Decode the post body
            DynamicTileContentPost request = Program.DecodePostBody<DynamicTileContentPost>(e);

            //Verify server
            var server = DeltaAuth.AuthenticateServer(request.server_id, request.server_creds);
            if (server == null)
                throw new Exception("Not authenticated.");

            //Create commit
            ContentTool.CommitContent(request);

            //Return OK
            Console.WriteLine("Created commit.");
            await Program.QuickWriteToDoc(e, "OK", "text/plain");
        }
    }
}
