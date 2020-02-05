
using System;
using System.Collections.Generic;
using System.Text;
using LibDeltaSystem.Db.System;
using LibDeltaSystem.Db.Content;
using MongoDB.Driver;
using System.Threading.Tasks;

/// <summary>
/// TODO: REFACCTOR THIS FILE
/// </summary>
namespace ArkWebMapMasterServer.NetEntities
{
    public class UsersMeReply
    {
        public string screen_name;
        public string profile_image_url;
        public string id;
        public string steam_id;
        public DbUserSettings user_settings;
        public List<UsersMeReply_Server> servers;
        public List<UsersMeReply_Cluster> clusters;

        public UsersMeReply()
        {

        }

        public async Task MakeUsersMe(DbUser u)
        {
            //Set basic values
            screen_name = u.screen_name;
            profile_image_url = u.profile_image_url;
            id = u.id;
            steam_id = u.steam_id;
            user_settings = u.user_settings;

            //Convert servers
            servers = new List<UsersMeReply_Server>();
            List<string> clusterIds = new List<string>();
            var found_servers = await u.GetGameServersAsync(Program.connection);
            foreach(var id in found_servers)
            {
                //Get server by ID
                var converted = MakeServer(id.Item1, id.Item2, u);
                servers.Add(converted);
                if (converted.cluster_id != null && !clusterIds.Contains(converted.cluster_id))
                    clusterIds.Add(converted.cluster_id);
            }

            //Convert clusters
            clusters = new List<UsersMeReply_Cluster>();
            foreach (var c in clusterIds)
                clusters.Add(new UsersMeReply_Cluster(await DbCluster.GetClusterById(Program.connection, MongoDB.Bson.ObjectId.Parse(c))));
        }

        public static UsersMeReply_Server MakeServer(DbServer s, DbPlayerProfile ps, DbUser user)
        {
            //If this server has never sent a status, skip
            UsersMeReply_Server reply = new UsersMeReply_Server();
            reply.display_name = s.display_name;
            reply.image_url = s.image_url;
            reply.owner_uid = s.owner_uid;
            reply.id = s.id;
            reply.has_ever_gone_online = true;
            reply.cluster_id = s.cluster_id;

            if(ps != null)
            {
                reply.tribeId = ps.tribe_id;
                reply.tribeName = s.GetTribeAsync(Program.connection, ps.tribe_id).GetAwaiter().GetResult().tribe_name;
                reply.arkName = ps.name;
            }

            string base_endpoint = Program.connection.config.hosts.master + "/api" +$"/servers/{s.id}/";
            reply.endpoint_createsession = Program.config.endpoint_echo+$"/{s.id}/" + "create_session";

            reply.map_id = s.latest_server_map;
            reply.map_name = reply.map_id;

            //Get the user prefs, if they exist. If they don't, generate one on the fly
            {
                var filterBuilder = Builders<DbSavedUserServerPrefs>.Filter;
                var filter = filterBuilder.Eq("server_id", s.id) & filterBuilder.Eq("user_id", user.id);
                var results = Program.connection.system_saved_user_server_prefs.Find(filter).FirstOrDefault();
                if(results != null)
                {
                    reply.user_prefs = results.payload;
                } else
                {
                    reply.user_prefs = new LibDeltaSystem.Db.System.Entities.SavedUserServerPrefs
                    {
                        x = 128,
                        y = -128,
                        z = 2,
                        map = 0,
                        drawable_map = null
                    };
                }
            }

            return reply;
        }

        public class UsersMeReply_Cluster
        {
            public string name;
            public string id;

            public UsersMeReply_Cluster(DbCluster c)
            {
                name = c.name;
                id = c.id;
            }
        }
    }
}
