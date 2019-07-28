using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkBridgeSharedEntities.Entities.Master
{
    public class DinoTribeSettings
    {
        /// <summary>
        /// In format {server id}/{tribe id}/{dino id}
        /// </summary>
        [JsonIgnore]
        public string _id { get; set; }

        public string server_id { get; set; }
        public int tribe_id { get; set; }
        public ulong dino_id { get; set; }

        public string notes { get; set; }
        public int? group_color { get; set; }
    }
}
