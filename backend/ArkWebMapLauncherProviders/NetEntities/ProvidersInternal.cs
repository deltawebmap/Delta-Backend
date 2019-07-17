using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapLauncherProviders.NetEntities
{
    public class InternalMachineConfigResponse
    {
        public string id;
        public ArkManagerProfile profile;
        public ArkManagerServer[] servers;
        public ArkManagerMachine machine;
        public Dictionary<string, InternalMachineConfigResponseServerInfo> linked_servers;
    }

    public class InternalMachineConfigResponseServerInfo
    {
        public string id;
        public byte[] creds;
    }

    public class ArkManagerProfile
    {
        /// <summary>
        /// The image for this hosting provider.
        /// </summary>
        public string wide_image_url { get; set; }

        /// <summary>
        /// The name of this provider
        /// </summary>
        public string name { get; set; }
    }

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

    public class ArkManagerServerGame_ARK
    {
        /// <summary>
        /// Name of the map
        /// </summary>
        public string map_name { get; set; }

        /// <summary>
        /// Path of the map
        /// </summary>
        public string map_path { get; set; }
    }
}
