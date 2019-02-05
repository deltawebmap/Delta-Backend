using System;
using System.Collections.Generic;
using System.Text;

namespace ArkHttpServer.Entities
{
    public class MasterServerArkUser
    {
        //User info
        public string profile_image_url { get; set; } //URL to profile image
        public string screen_name { get; set; } //Username displayed
        public List<string> servers { get; set; } //Servers the user is in, by ID
        public string id { get; set; } //ID of the user

        public bool is_steam_verified { get; set; }
        public string steam_id { get; set; }
    }
}
