using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapAnalytics.PersistEntities
{
    public class ActionEntry
    {
        [JsonProperty("id")]
        public string _id { get; set; }
        public long time { get; set; } //Time, UTC

        public int logger_version { get; set; } //Version of the logger

        public string access_token { get; set; } //Token
        public string user_id { get; set; } //ID of the user

        public string client_name { get; set; } //The name of the client, such as "web", or "android"
        public string client_version { get; set; } //The version of the client
        public string client_view { get; set; } //The page we're on
        public string client_details { get; set; } //User agent

        public string topic { get; set; }
        public string server_id { get; set; }
        public bool server_online { get; set; }
        public Dictionary<string, string> extras { get; set; }
    }
}
