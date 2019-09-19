using System;
using System.Collections.Generic;
using System.Text;

namespace ArkBridgeSharedEntities.Entities.Master
{
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
    }
}
