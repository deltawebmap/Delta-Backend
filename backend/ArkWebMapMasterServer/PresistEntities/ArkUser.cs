using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

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
        public List<string> servers { get; set; } //Servers the user is in, by ID
        [JsonProperty("id")]
        public string _id { get; set; } //ID of the user

        public void Update()
        {
            Users.UserAuth.GetCollection().Update(this);
        }
    }

    public enum ArkUserSigninMethod
    {
        None, //Used when auth has not yet been configured.
        UsernamePassword
    }
}
