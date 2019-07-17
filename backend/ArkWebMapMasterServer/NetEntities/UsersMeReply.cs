using ArkBridgeSharedEntities.Entities;
using ArkWebMapMasterServer.PresistEntities;
using System;
using System.Collections.Generic;
using System.Text;

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

        public UsersMeReply(ArkUser u, bool filterHiddenServers, bool doPingServers)
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
                var converted = new UsersMeReply_Server(u, id.Item1, id.Item2, doPingServers);
                if ((!converted.is_hidden || !filterHiddenServers) && converted.has_ever_gone_online)
                    servers.Add(converted);
            }

        }
    }

    public class UsersMeReply_Server
    {
        public string display_name;
        public string image_url;
        public string owner_uid;
        public string id;

        public int tribeId;
        public string tribeName;
        public string arkName;

        public string map_id;
        public string map_name;

        public bool has_ever_gone_online;
        public bool is_hidden;
        public bool is_public;
        public ArkPublishedServerListing public_listing;
        public DateTime lastReportTime;

        public string endpoint_createsession;
        public string endpoint_offline_data;
        public string endpoint_hub;

        public List<string> enabled_notifications;

        public bool is_online;

        public UsersMeReply_Server()
        {

        }

        public UsersMeReply_Server(ArkUser u, ArkServer s, ArkSlaveReport_PlayerAccount ps, bool doPing)
        {
            //If this server has never sent a status, skip
            if (!s.has_server_report)
            {
                has_ever_gone_online = false;
                return;
            }

            display_name = s.display_name;
            image_url = s.image_url;
            if (image_url == null)
                image_url = s.GetPlaceholderIcon();
            owner_uid = s.owner_uid;
            id = s._id;
            has_ever_gone_online = s.has_server_report;
            is_hidden = u.hidden_servers.Contains(s._id);
            lastReportTime = new DateTime(s.latest_server_report_downloaded);

            if(ps != null)
            {
                tribeId = ps.player_tribe_id;
                tribeName = ps.player_tribe_name;
                arkName = ps.player_name;
            }

            string base_endpoint = $"https://ark.romanport.com/api/servers/{id}/";
            endpoint_hub = base_endpoint + "hub";
            endpoint_createsession = $"https://lightspeed-ark.romanport.com/{id}/" + "create_session";
            endpoint_offline_data = base_endpoint + "offline_data";

            map_id = s.latest_server_map;
            map_name = map_id;
            is_online = Program.onlineServers.Contains(s._id);

            //Convert permissions
            List<ArkNotificationChannel> notifications = u.GetServerNotificationSettings(s._id);
            enabled_notifications = new List<string>();
            foreach (ArkNotificationChannel ss in notifications)
                enabled_notifications.Add(ss.ToString());

            //Get the published server listing, if we have it
            if(s.is_published)
            {
                ArkPublishedServerListing listing = ServerPublishingManager.GetPublishedServer(s._id);
                public_listing = listing;
                is_public = listing != null;
            }
        }
    }
}
