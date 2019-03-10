using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapSubServerUpdater
{
    public class RemoteConfigFile
    {
        public RemoteConfigFile_Release latest_release;
        public RemoteConfigFile_SubServerConfig sub_server_config;
    }

    public class RemoteConfigFile_Release
    {
        public float version_id;
        public string release_notes;
        public string download_page;
        public Dictionary<string, RemoteConfigFile_Release_Binary> binaries;
    }

    public class RemoteConfigFile_Release_Binary
    {
        public string url;
        public string updater_url;
        public string updater_cmd;
        public string exe_name;
        public string virustotal;
    }

    public class RemoteConfigFile_SubServerConfig
    {
        public float minimum_release_id;
        public string startup_message;
        public RemoteConfigFile_SubServerConfig_Endpoints endpoints;
    }

    public class RemoteConfigFile_SubServerConfig_Endpoints
    {
        public string server_setup_proxy;
        public string base_bridge_url;
        public string server_api_prefix;
    }
}
