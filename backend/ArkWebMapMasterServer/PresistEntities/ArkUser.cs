using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using ArkBridgeSharedEntities.Entities;

namespace ArkWebMapMasterServer.PresistEntities
{
    public class ArkUser
    {
        //AUTH
        public ArkUserSigninMethod auth_method { get; set; }
        [JsonIgnore()]
        public IAuthMethod auth { get; set; }

        //User info
        public string profile_image_url { get; set; } //URL to profile image
        public string screen_name { get; set; } //Username displayed
        public List<string> my_servers { get; set; } //Servers the user owns, by ID
        public List<string> joined_servers { get; set; } //Servers the user is in. Run GetServers to obtain a new list.
        [JsonProperty("id")]
        public string _id { get; set; } //ID of the user
        public List<string> hidden_servers { get; set; }

        public bool is_steam_verified { get; set; }
        public string steam_id { get; set; }

        public ArkUserSettings user_settings { get; set; } = new ArkUserSettings();

        public List<string> notification_tokens { get; set; }
        public Dictionary<string, List<ArkNotificationChannel>> enabled_notifications { get; set; } //Key: Server ID

        public void Update()
        {
            Users.UserAuth.GetCollection().Update(this);
        }

        public List<Tuple<ArkServer, ArkSlaveReport_PlayerAccount>> GetServers(bool excludeHidden = false)
        {
            //Loop through all servers and see if I am in their accounts
            if (!is_steam_verified)
                return new List<Tuple<ArkServer, ArkSlaveReport_PlayerAccount>>();

            if (hidden_servers == null)
                hidden_servers = new List<string>();

            List<Tuple<ArkServer, ArkSlaveReport_PlayerAccount>> output = new List<Tuple<ArkServer, ArkSlaveReport_PlayerAccount>>();
            var serversToTry = Servers.ArkSlaveServerSetup.GetCollection().Find(x => x.has_server_report || x.owner_uid == _id).ToArray();
            List<string> output_ids = new List<string>();
            foreach(var s in serversToTry)
            {
                bool check = s.owner_uid == _id;
                ArkSlaveReport_PlayerAccount playerAcc = null;
                if(s.latest_server_local_accounts != null)
                {
                    var results = s.latest_server_local_accounts.Where(x => x.player_steam_id == steam_id);
                    if (results.Count() >= 1)
                    {
                        playerAcc = results.First();
                        check = true;
                    }
                }

                if (check)
                {
                    if((!hidden_servers.Contains(s._id) || !excludeHidden) && !s.is_deleted) {
                        output.Add(new Tuple<ArkServer, ArkSlaveReport_PlayerAccount>(s, playerAcc));
                        if (!output_ids.Contains(s._id))
                            output_ids.Add(s._id);
                    }
                }
            }
            joined_servers = output_ids;
            Update();
            return output;
        }

        public List<ArkNotificationChannel> GetServerNotificationSettings(string serverId)
        {
            if (enabled_notifications == null)
                return ArkUserDefaults.defaultUserNotifications;
            if (!enabled_notifications.ContainsKey(serverId))
                return ArkUserDefaults.defaultUserNotifications;
            return enabled_notifications[serverId];
        }
    }

    public class ArkUserSettings
    {
        public List<string> custom_vulgar_words { get; set; } = new List<string>();
        public bool vulgar_filter_on { get; set; } = true;
        public bool vulgar_show_censored_on { get; set; } = false; //If this is on, blocked names will still show, but censored.
    }

    class ArkUserDefaults
    {
        public static readonly List<ArkNotificationChannel> defaultUserNotifications = new List<ArkNotificationChannel>
        {
            ArkNotificationChannel.BabyDino_FoodCritical,
            ArkNotificationChannel.BabyDino_FoodLow,
            ArkNotificationChannel.BabyDino_FoodStarving,
            ArkNotificationChannel.Tribe_TribeDinoDeath,
            ArkNotificationChannel.Tribe_TribeDinoTame
        };
    }

    public enum ArkUserSigninMethod
    {
        None, //Used when auth has not yet been configured.
        UsernamePassword,
        SteamProfile
    }
}
