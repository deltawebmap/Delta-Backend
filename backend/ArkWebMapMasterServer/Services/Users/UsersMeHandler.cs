using LibDeltaSystem.Db;
using LibDeltaSystem.Db.Content;
using LibDeltaSystem.Db.System;
using LibDeltaSystem.Db.System.Entities;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Users
{
    public static class UsersMeHandler
    {
        public static async Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, DbUser u)
        {
            //Set up our basic values
            UsersMeResponse response = new UsersMeResponse
            {
                screen_name = u.screen_name,
                profile_image_url = u.profile_image_url,
                id = u.id,
                steam_id = u.steam_id,
                user_settings = u.user_settings,
                servers = new List<UsersMeReply_Server>(),
                clusters = new List<UsersMeReply_Cluster>()
            };

            //Fetch and convert our servers
            var servers = await u.GetGameServersAsync(Program.connection);
            List<string> clusterIds = new List<string>();
            List<ObjectId> serverIds = new List<ObjectId>();
            foreach(var s in servers)
            {
                //Split
                int tribeId = s.Item2.tribe_id;
                DbServer server = s.Item1;

                //Add server
                serverIds.Add(s.Item1._id);
                
                //Get tribe info
                var tribe = await Program.connection.GetTribeByTribeIdAsync(s.Item1.id, s.Item2.tribe_id);

                //Get my location
                DbVector3 myPos = null;
                if (s.Item2.x != null && s.Item2.y != null && s.Item2.z != null)
                    myPos = new DbVector3
                    {
                        x = s.Item2.x.Value,
                        y = s.Item2.y.Value,
                        z = s.Item2.z.Value
                    };

                //Get map info
                string mapName = null;
                var mapData = await s.Item1.GetMapEntryAsync(Program.connection);
                if (mapData != null)
                    mapName = mapData.displayName;

                //Get closed reason, if any
                int close = -1;
                for(int i = 31; i>=0; i--)
                {
                    if (s.Item1.CheckLockFlag(i))
                        close = i;
                }

                //Check pseudo flags
                if (mapData == null)
                    close = 32; //MAP_NOT_SUPPORTED

                //Create response
                UsersMeReply_Server sResponse = new UsersMeReply_Server
                {
                    display_name = s.Item1.display_name,
                    image_url = s.Item1.image_url,
                    owner_uid = s.Item1.owner_uid,
                    cluster_id = s.Item1.cluster_id,
                    id = s.Item1.id,
                    map_id = s.Item1.latest_server_map,
                    permissions = s.Item1.GetPermissionFlagList(),
                    closed_reason = close,
                    user_prefs = await s.Item1.GetUserPrefs(Program.connection, u.id),
                    my_location = myPos,
                    ark_id = s.Item2.ark_id.ToString(),
                    target_tribe = tribe,
                    is_admin = s.Item1.CheckIsUserAdmin(u),
                    has_tribe = s.Item1 != null
                };

                //Add cluster if not added
                if (s.Item1.cluster_id != null && !clusterIds.Contains(s.Item1.cluster_id))
                    clusterIds.Add(s.Item1.cluster_id);

                //Add server
                response.servers.Add(sResponse);
            }

            //Add clusters
            foreach(var c in clusterIds)
            {
                //Get cluster data
                var cluster = await DbCluster.GetClusterById(Program.connection, MongoDB.Bson.ObjectId.Parse(c));

                //Add cluster data
                response.clusters.Add(new UsersMeReply_Cluster
                {
                    id = cluster.id,
                    name = cluster.name
                });
            }

            //Get messages
            response.alerts = await (await u.GetAlertBanners(Program.connection, serverIds)).ToListAsync();

            //Write response
            await Program.QuickWriteJsonToDoc(e, response);
        }

        class UsersMeResponse
        {
            public string screen_name;
            public string profile_image_url;
            public string id;
            public string steam_id;
            public DbUserSettings user_settings;
            public List<UsersMeReply_Server> servers;
            public List<UsersMeReply_Cluster> clusters;
            public List<DbAlertBanner> alerts;
        }

        public class UsersMeReply_Server
        {
            public string display_name;
            public string image_url;
            public string owner_uid;
            public string cluster_id;
            public string id;

            public DbTribe target_tribe; //The tribe this user belongs to
            public DbVector3 my_location; //The current location of the user
            public string ark_id;

            public string map_id;

            public bool[] permissions;
            public int closed_reason; //https://docs.google.com/spreadsheets/d/1zQ_r86uyDAvwAtEg0135rL6g2lHqhPtYAgFdJrL3vZc/edit

            public SavedUserServerPrefs user_prefs;

            public bool is_admin;
            public bool has_tribe; 
        }

        public class UsersMeReply_Cluster
        {
            public string name;
            public string id;
        }
    }
}
