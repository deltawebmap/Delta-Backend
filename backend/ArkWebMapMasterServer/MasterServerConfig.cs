using LibDeltaSystem;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapMasterServer
{
    public class MasterServerConfig
    {
        public string github_api_key;
        public string database_config_path;
        public string map_config_path;
        public int listen_port;

        public string endpoint_master; //https://deltamap.net/api
    }
}
