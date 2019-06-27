using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapMasterServer.PresistEntities.Managers
{
    public class ArkManagerServer
    {
        /// <summary>
        /// ID of this object
        /// </summary>
        [JsonProperty("id")]
        public string _id { get; set; }

        /// <summary>
        /// User-Provided name
        /// </summary>
        public string name { get; set; }
        
        /// <summary>
        /// COULD BE NULL. ID of the ArkManagerClient
        /// </summary>
        public string client_id { get; set; }

        /// <summary>
        /// The machine this server is running on.
        /// </summary>
        public string machine_id { get; set; }

        /// <summary>
        /// The ID of the manager that operates this machine.
        /// </summary>
        public string manager_id { get; set; }

        /// <summary>
        /// The ID of the actual server in our database
        /// </summary>
        public string linked_id { get; set; }

        /// <summary>
        /// Creation time
        /// </summary>
        public long time { get; set; }

        /// <summary>
        /// Settings for this game.
        /// </summary>
        public ArkManagerServerGame_ARK game_settings { get; set; }
    }
}
