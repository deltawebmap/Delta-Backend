using System;
using System.Collections.Generic;
using System.Text;

namespace ArkBridgeSharedEntities.Entities.RemoteConfig
{
    public class RemoteConfigFile
    {
        public RemoteConfigFile_SubServerConfig sub_server_config;
    }

    public class RemoteConfigFile_SubServerConfig
    {
        public float minimum_release_id;
        public string startup_message;
        public int offline_sync_policy_seconds; //Time between each report for offline data
        public RemoteConfigFile_SubServerConfig_Endpoints endpoints;
    }

    public class RemoteConfigFile_SubServerConfig_Endpoints
    {
        public string server_setup_proxy;
        public string base_bridge_url;
        public string server_api_prefix;
    }
}
