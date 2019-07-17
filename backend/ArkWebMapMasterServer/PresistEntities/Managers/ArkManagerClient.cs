using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapMasterServer.PresistEntities.Managers
{
    public class ArkManagerClient
    {
        /// <summary>
        /// ID of thjs client
        /// </summary>
        [JsonProperty("id")]
        public string _id { get; set; }

        /// <summary>
        /// Code to send to clients to link their account to this. Could be null.
        /// </summary>
        public string invite_code { get; set; }

        /// <summary>
        /// Linked user ID. Could be null.
        /// </summary>
        public string linked_userid { get; set; }

        /// <summary>
        /// Name set by provider.
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// Provider ID
        /// </summary>
        public string manager_id { get; set; }
    }
}
