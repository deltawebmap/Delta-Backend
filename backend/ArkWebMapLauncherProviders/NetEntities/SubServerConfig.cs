using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapLauncherProviders.NetEntities
{
    public class ArkSlaveConfig
    {
        public int web_port;
        public ServerConfigFile child_config;
        public ArkSlaveConnection auth;
        public bool debug_mode; //Enables additional logging and disables the security check when HTTP requests come in.
    }

    public class ArkSlaveConnection
    {
        public string creds;
        public string id;
    }

    public class ServerConfigFile
    {
        public string save_location = @"C:\Program Files (x86)\Steam\steamapps\common\ARK\ShooterGame\Saved\SavedArks\";
        public string save_map = @"Extinction.ark";
        public string resources_url = "https://ark.romanport.com/resources";

        public List<string> base_permissions;
        public int permissions_version;
        public bool is_demo_server; //This will disable tribes and make everyone use the same tribe id
        public int demo_tribe_id; //The ID of the demo tribe to use.
    }
}
