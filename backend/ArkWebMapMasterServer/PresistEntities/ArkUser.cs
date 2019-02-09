using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

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

        public void Update()
        {
            Users.UserAuth.GetCollection().Update(this);
        }

        public List<ArkServer> GetServers(bool excludeHidden = false)
        {
            //Loop through all servers and see if I am in their accounts
            if (!is_steam_verified)
                return new List<ArkServer>();

            if (hidden_servers == null)
                hidden_servers = new List<string>();

            List<ArkServer> output = new List<ArkServer>();
            var serversToTry = Servers.ArkSlaveServerSetup.GetCollection().Find(x => x.has_server_report || x.owner_uid == _id).ToArray();
            List<string> output_ids = new List<string>();
            foreach(var s in serversToTry)
            {
                bool check = s.owner_uid == _id;
                if (!check)
                    check = s.latest_server_local_accounts.Count(x => x.player_steam_id == steam_id) >= 1;
                if (check)
                {
                    if(!hidden_servers.Contains(s._id) || !excludeHidden) {
                        output.Add(s);
                        if (!output_ids.Contains(s._id))
                            output_ids.Add(s._id);
                    }
                }
            }
            joined_servers = output_ids;
            Update();
            return output;
        }
    }

    public enum ArkUserSigninMethod
    {
        None, //Used when auth has not yet been configured.
        UsernamePassword,
        SteamProfile
    }
}
