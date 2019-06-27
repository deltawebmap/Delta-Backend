using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapMasterServer.PresistEntities.Managers
{
    public class ArkManagerMachine
    {
        /// <summary>
        /// The ID of this
        /// </summary>
        [JsonProperty("id")]
        public string _id { get; set; }

        /// <summary>
        /// The manager that owns this machine
        /// </summary>
        public string ownerId { get; set; }

        /// <summary>
        /// The name that the user chooses
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// The location that the user chooses
        /// </summary>
        public string location { get; set; }

        /// <summary>
        /// Time this was created.
        /// </summary>
        public long created { get; set; }
    }
}
