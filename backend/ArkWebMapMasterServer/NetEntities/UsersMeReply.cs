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

        public List<UsersMeReply_Server> servers;

        public UsersMeReply(ArkUser u, bool filterHiddenServers, bool doPingServers)
        {
            //Set basic values
            screen_name = u.screen_name;
            profile_image_url = u.profile_image_url;
            id = u._id;

            //Check
            if (u.hidden_servers == null)
                u.hidden_servers = new List<string>();

            //Convert servers
            servers = new List<UsersMeReply_Server>();
            var found_servers = u.GetServers(false);
            foreach(var id in found_servers)
            {
                //Get server by ID
                var converted = new UsersMeReply_Server(u, id, doPingServers);
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

        public bool has_ever_gone_online;
        public bool is_hidden;

        public string endpoint_ping;
        public string endpoint_leave;
        public string endpoint_createsession;
        public string endpoint_createinvite;

        public List<string> enabled_notifications;
        public ArkServerReply ping_status;

        public UsersMeReply_Server(ArkUser u, ArkServer s, bool doPing)
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

            string base_endpoint = $"https://ark.romanport.com/api/servers/{id}/";
            endpoint_createinvite = base_endpoint + "invites/create";
            endpoint_createsession = base_endpoint + "create_session";
            endpoint_leave = base_endpoint + "leave";
            endpoint_ping = base_endpoint.TrimEnd('/');

            //Convert permissions
            List<ArkNotificationChannel> notifications = u.GetServerNotificationSettings(s._id);
            enabled_notifications = new List<string>();
            foreach (ArkNotificationChannel ss in notifications)
                enabled_notifications.Add(ss.ToString());

            //Ping server
            if(doPing)
                ping_status = new ArkServerReply(s, null, 600);
        }
    }
}
