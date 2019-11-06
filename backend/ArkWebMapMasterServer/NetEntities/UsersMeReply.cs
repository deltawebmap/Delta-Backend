using ArkBridgeSharedEntities.Entities;
using ArkWebMapMasterServer.PresistEntities;
using System;
using System.Collections.Generic;
using System.Text;
using ArkBridgeSharedEntities.Entities.Master;
using LibDeltaSystem.Db.System;
using LibDeltaSystem.Db.Content;
using MongoDB.Driver;

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

        public UsersMeReply()
        {

        }

        public void MakeUsersMe(DbUser u, bool filterHiddenServers)
        {
            //Set basic values
            screen_name = u.screen_name;
            profile_image_url = u.profile_image_url;
            id = u.id;
            steam_id = u.steam_id;
            user_settings = u.user_settings;

            //Convert servers
            servers = new List<UsersMeReply_Server>();
            var found_servers = u.GetGameServers();
            foreach(var id in found_servers)
            {
                //Get server by ID
                var converted = MakeServer(id.Item1, id.Item2, u);
                servers.Add(converted);
            }

        }

        public void MakeDummyUsersMe()
        {
            //Used for demo servers. Make a fake users me for the client
            screen_name = "Delta Demo";
            profile_image_url = "";
            id = null;
            steam_id = null;
            user_settings = new DbUserSettings();
            servers = new List<UsersMeReply_Server>();
            /*servers.Add(MakeServer(Servers.ArkSlaveServerSetup.GetCollection().FindById(DemoServerData.DEMO_SERVER_ID), new ArkSlaveReport_PlayerAccount
            {
                allow_player = true,
                player_name = "Demo User",
                player_steam_id = null,
                player_tribe_id = DemoServerData.DEMO_SERVER_TRIBE_ID,
                player_tribe_name = "Demo Tribe"
            }));*/

            //TODO!!
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

            if(ps != null)
            {
                reply.tribeId = ps.tribe_id;
                reply.tribeName = s.GetTribeAsync(ps.tribe_id).GetAwaiter().GetResult().tribe_name;
                reply.arkName = ps.name;
            }

            string base_endpoint = $"https://deltamap.net/api/servers/{s.id}/";
            reply.endpoint_createsession = $"https://echo-content.deltamap.net/{s.id}/" + "create_session";

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
    }
}
