using ArkBridgeSharedEntities.Entities;
using ArkWebMapMasterServer.PresistEntities;
using System;
using System.Collections.Generic;
using System.Text;
using ArkBridgeSharedEntities.Entities.Master;

namespace ArkWebMapMasterServer.NetEntities
{
    public class UsersMeReply
    {
        public string screen_name;
        public string profile_image_url;
        public string id;
        public string steam_id;
        public ArkUserSettings user_settings;

        public List<UsersMeReply_Server> servers;

        public UsersMeReply()
        {

        }

        public UsersMeReply(ArkUser u, bool filterHiddenServers)
        {
            //Set basic values
            screen_name = u.screen_name;
            profile_image_url = u.profile_image_url;
            id = u._id;
            steam_id = u.steam_id;
            user_settings = u.user_settings;

            //Check
            if (u.hidden_servers == null)
                u.hidden_servers = new List<string>();

            //Convert servers
            servers = new List<UsersMeReply_Server>();
            var found_servers = u.GetServers(false);
            foreach(var id in found_servers)
            {
                //Get server by ID
                var converted = MakeServer(u, id.Item1, id.Item2);
                if ((!converted.is_hidden || !filterHiddenServers) && converted.has_ever_gone_online)
                    servers.Add(converted);
            }

        }

        public static UsersMeReply_Server MakeServer(ArkUser u, ArkServer s, ArkSlaveReport_PlayerAccount ps)
        {
            //If this server has never sent a status, skip
            UsersMeReply_Server reply = new UsersMeReply_Server();
            reply.display_name = s.display_name;
            reply.image_url = s.image_url;
            if (reply.image_url == null)
                reply.image_url = s.GetPlaceholderIcon();
            reply.owner_uid = s.owner_uid;
            reply.id = s._id;
            reply.has_ever_gone_online = s.has_server_report;
            reply.is_hidden = u.hidden_servers.Contains(s._id);
            reply.lastReportTime = new DateTime(s.latest_server_report_downloaded);

            if(ps != null)
            {
                reply.tribeId = ps.player_tribe_id;
                reply.tribeName = ps.player_tribe_name;
                reply.arkName = ps.player_name;
            }

            string base_endpoint = $"https://deltamap.net/api/servers/{s._id}/";
            reply.endpoint_hub = base_endpoint + "hub";
            reply.endpoint_createsession = $"https://lightspeed.deltamap.net/{s._id}/" + "create_session";
            reply.endpoint_offline_data = base_endpoint + "offline_data";

            reply.map_id = s.latest_server_map;
            reply.map_name = reply.map_id;
            reply.is_online = Program.onlineServers.Contains(s._id);

            //Get the published server listing, if we have it
            if(s.is_published)
            {
                ArkPublishedServerListing listing = ServerPublishingManager.GetPublishedServer(s._id);
                reply.public_listing = listing;
                reply.is_public = listing != null;
            }

            return reply;
        }
    }
}
