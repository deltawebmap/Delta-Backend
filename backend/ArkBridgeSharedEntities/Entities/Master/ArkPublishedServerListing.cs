using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkBridgeSharedEntities.Entities.Master
{
    public class ArkPublishedServerListing
    {
        [JsonProperty("server_id")]
        public string _id { get; set; } //ID of the server this is refrencing 

        public string banner_url { get; set; } //Banner URL to image
        [JsonIgnore]
        public string banner_removal_token { get; set; } //Token to remove the banner URL

        public string icon_url { get; set; } //Banner URL to image
        [JsonIgnore]
        public string icon_removal_token { get; set; } //Token to remove the banner URL

        public string map_name { get; set; } //INTERNAL map name
        public string display_name { get; set; } //Name shown to users
        public string display_motd { get; set; } //Subtext to name
        public bool is_published { get; set; } //Is published and shown

        public ArkPublishedServerLocation location { get; set; } = ArkPublishedServerLocation.NorthAmerica;
        public string language { get; set; } = "EN";

        public string discord_code { get; set; }
        public DiscordInvite discord_invite { get; set; }

        public string ip_address { get; set; } //IP to the server
        public bool is_ip_verified { get; set; } //Is the IP verified

        public int saved_total_players { get; set; } //Total players on the server
        public int saved_active_players { get; set; } //Total players online in the last 4 days

        public ArkPublishedServerFlags flags { get; set; }
        public ArkPublishedServerSettings settings { get; set; }
    }

    public class ArkPublishedServerFlags
    {
        public bool is_modded { get; set; }
        public bool is_pvp { get; set; }
        public bool is_small_tribes { get; set; }
        public bool is_shop { get; set; }
        public bool is_cluster { get; set; }
    }

    public class ArkPublishedServerSettings
    {
        public float taming_speed { get; set; } = 1;
        public float xp_multiplier { get; set; } = 1;
        public float gather_multiplier { get; set; } = 1;
        public float maturation_multiplier { get; set; } = 1;
        public float breeding_multiplioer { get; set; } = 1;
    }

    public enum ArkPublishedServerLocation
    {
        NorthAmerica,
        SouthAmerica,
        Europe,
        Asia,
        Australia,
        Africa
    }
}
