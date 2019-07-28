using ArkBridgeSharedEntities.Entities.Master;
using ArkWebMapMasterServer.PresistEntities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapMasterServer.NetEntities
{
    //Sent in a POST body, usually
    public class PublishedServerEdit
    {
        public string display_name;
        public string display_motd;

        public string discord_code;

        public string icon_token;
        public string banner_token;

        public string ip_address;

        public ArkPublishedServerLocation? location { get; set; }
        public string language { get; set; }

        public ArkPublishedServerFlags flags;
        public ArkPublishedServerSettings settings;
    }
}
