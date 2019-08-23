using ArkBridgeSharedEntities.Requests;
using ArkWebMapDynamicTiles.Entities;
using ArkWebMapDynamicTiles.MapSessions;
using ArkWebMapMasterServer.NetEntities;
using LibDelta;
using Newtonsoft.Json;
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
                //Split the path
                string[] split = e.Request.Path.ToString().Split('/');

                //Check if this is one of our pathnames
                if (e.Request.Path.ToString() == "/upload") { await OnContentPost(e); return; }
                if (e.Request.Path.ToString() == "/commit") { await OnCommitPost(e); return; }
                if (e.Request.Path.ToString() == "/structure_sizes.json") { await Program.QuickWriteToDoc(e, JsonConvert.SerializeObject(Program.structure_size_map), "application/json"); return; }
                if (e.Request.Path.ToString().StartsWith("/create/")) { await CreateSession(e, split); return; }
                if (e.Request.Path.ToString().StartsWith("/heartbeat/")) { await HeartbeatSession(e, split); return; }
                if (e.Request.Path.ToString().StartsWith("/act/")) { await ActSession(e, split); return; }

                //Unknown
                await Program.QuickWriteToDoc(e, "Endpoint Not Found", "text/plain", 404);
                return;
            } catch (Exception ex)
            {
                Console.WriteLine($"ERROR {ex.Message} {ex.StackTrace}");
                await Program.QuickWriteToDoc(e, "Internal Server Error", "text/plain", 500);
            }
        }

        private static async Task CreateSession(Microsoft.AspNetCore.Http.HttpContext e, string[] split)
        {
            if (split.Length != 4)
            {
                await Program.QuickWriteToDoc(e, "Invalid URL Structure", "text/plain", 400);
                return;
            }
            string serverId = split[2];
            string mapType = split[3];

            //Authenticate this user
            UsersMeReply user = await AuthenticateUser(e, split);
            if (user == null)
                return;

            //Ensure this user has this server on their list
            if (user.servers.Where(i => i.id == serverId).Count() != 1)
            {
                await Program.QuickWriteToDoc(e, "You Are Not A Member Of This Server", "text/plain", 403);
                return;
            }
            var server = user.servers.Where(i => i.id == serverId).First();

            //Load data for this server
            ContentMetadata commit = ContentTool.GetCommit(serverId);
            if (commit == null)
            {
                await Program.QuickWriteToDoc(e, "This Server Has Not Uploaded Data Yet", "text/plain", 404);
                return;
            }

            //Switch off to the map to handle this
            MapSession session;
            if (mapType == "structures")
                session = new StructureMapSession();
            else
            {
                await Program.QuickWriteToDoc(e, "Unknown Map Type", "text/plain", 404);
                return;
            }

            //Set some data
            session.user_id = user.id;
            session.tribe_id = server.tribeId;
            session.server_id = server.id;
            session.last_heartbeat = DateTime.UtcNow;

            //Ensure this is an okay version
            if (commit.version < GLOBAL_MIN_DATA_VERSION || commit.version < session.GetMinDataVersion())
            {
                await Program.QuickWriteToDoc(e, "The Server Data Is Too Old", "text/plain", 404);
                return;
            }

            //Load session
            await session.OnCreate(e, server, commit);

            //Add to sessions list
            string token = SessionTool.AddSession(session);

            //Create data and write it
            SessionCreateData response = new SessionCreateData
            {
                data_revision = commit.revision,
                data_time = new DateTime(commit.time),
                data_version = commit.version,
                heartbeat_policy_ms = Program.HEARTBEAT_POLICY_MS,
                token = token,
                url_map = Program.SESSION_ROOT + "act/" + token + "/{x}_{y}_{z}",
                url_heartbeat = Program.SESSION_ROOT + "heartbeat/" + token
            };
            await Program.QuickWriteToDoc(e, JsonConvert.SerializeObject(response, Formatting.Indented), "application/json");
        }

        private static async Task ActSession(Microsoft.AspNetCore.Http.HttpContext e, string[] split)
        {
            //Check URL format
            if (split.Length != 4)
            {
                await Program.QuickWriteToDoc(e, "Invalid URL Structure", "text/plain", 400);
                return;
            }

            //Get location
            string[] coords = split[3].Split('_');
            if (coords.Length != 3)
            {
                await Program.QuickWriteToDoc(e, "Invalid URL Structure", "text/plain", 400);
                return;
            }
            float x = float.Parse(coords[0]);
            float y = float.Parse(coords[1]);
            float z = float.Parse(coords[2]);

            //Try and find the session
            MapSession s = SessionTool.GetSession(split[2]);

            //Check to make sure that this is within zoom bounds
            if (z > s.GetMaxZoom())
            {
                await Program.QuickWriteToDoc(e, "Zoom Level Too High", "text/plain", 400);
                return;
            }

            //Check
            if (s == null)
            {
                await Program.QuickWriteToDoc(e, "Session Expired", "text/plain", 410);
                return;
            }

            //Let the service handle it
            await s.OnHttpRequest(e, x, y, z);
        }

        private static async Task HeartbeatSession(Microsoft.AspNetCore.Http.HttpContext e, string[] split)
        {
            //Check URL format
            if (split.Length != 3)
            {
                await Program.QuickWriteToDoc(e, "Invalid URL Structure", "text/plain", 400);
                return;
            }

            //Try and find the session
            MapSession s = SessionTool.GetSession(split[2]);

            //Check
            if(s == null)
            {
                await Program.QuickWriteToDoc(e, "Session Expired", "text/plain", 410);
                return;
            }

            //Update
            s.last_heartbeat = DateTime.UtcNow;

            //Respond OK
            await Program.QuickWriteToDoc(e, "OK", "text/plain", 200);
        }

        private static async Task<UsersMeReply> AuthenticateUser(Microsoft.AspNetCore.Http.HttpContext e, string[] pathSplit)
        {
            //Find the token
            string token = null;

            //Check the headers first
            if (e.Request.Headers.ContainsKey("authorization"))
                token = e.Request.Headers["authorization"].ToString().Substring("Bearer ".Length);

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
