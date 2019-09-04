using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace ArkWebMapMasterServer.PresistEntities
{
    public class ArkCluster
    {
        [JsonProperty("id")]
        public string _id { get; set; }

        public string name { get; set; }
        public string owner_id { get; set; }
    }
}
