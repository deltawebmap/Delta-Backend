using LibDeltaSystem;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapMasterServer
{
    public class MasterServerConfig
    {
        public string database_pathname;
        public string map_database_pathname;
        public string github_api_key;
        public Dictionary<string, string> system_server_keys; //Stores keys for each system service running on the network. Should be secure.
        public string database_config_path;
        public string map_config_path;
        public string gateway_key;
    }
}
