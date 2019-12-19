using LibDeltaSystem.Db.System;
using LibDeltaSystem.Db.System.Entities;
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
            var servers = await u.GetGameServersAsync();
            List<string> clusterIds = new List<string>();
            foreach(var s in servers)
            {
                //Get tribe info
                var tribe = await Program.connection.GetTribeByTribeIdAsync(s.Item1.id, s.Item2.tribe_id);

                //Get map info
                string mapName = null;
                if (Program.ark_maps.ContainsKey(s.Item1.latest_server_map))
                    mapName = Program.ark_maps[s.Item1.latest_server_map].displayName;

                //Get closed reason, if any
                int close = -1;
                for(int i = 31; i>=0; i--)
                {
                    if (s.Item1.CheckLockFlag(i))
                        close = i;
                }

                //Check pseudo flags
                if (!Program.ark_maps.ContainsKey(s.Item1.latest_server_map))
                    close = 32; //MAP_NOT_SUPPORTED

                //Create response
                UsersMeReply_Server sResponse = new UsersMeReply_Server
                {
                    display_name = s.Item1.display_name,
                    image_url = s.Item1.image_url,
                    owner_uid = s.Item1.owner_uid,
                    cluster_id = s.Item1.cluster_id,
                    id = s.Item1.id,
                    tribe_id = s.Item2.tribe_id,
                    tribe_name = tribe.tribe_name,
                    map_id = s.Item1.latest_server_map,
                    map_name = mapName,
                    permissions = s.Item1.GetPermissionFlagList(),
                    closed_reason = close,
                    user_prefs = await s.Item1.GetUserPrefs(u.id),
                    endpoint_createsession = Program.config.endpoint_echo + $"/{s.Item1.id}/" + "create_session"
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
        }

        public class UsersMeReply_Server
        {
            public string display_name;
            public string image_url;
            public string owner_uid;
            public string cluster_id;
            public string id;

            public int tribe_id;
            public string tribe_name;
            public string ark_name;

            public string map_id;
            public string map_name;

            public bool[] permissions;
            public int closed_reason; //https://docs.google.com/spreadsheets/d/1zQ_r86uyDAvwAtEg0135rL6g2lHqhPtYAgFdJrL3vZc/edit

            public SavedUserServerPrefs user_prefs;

            public string endpoint_createsession;
        }

        public class UsersMeReply_Cluster
        {
            public string name;
            public string id;
        }
    }
}
