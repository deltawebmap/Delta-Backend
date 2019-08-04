using System;
using System.Collections.Generic;
using System.Text;

namespace ArkHttpServer
{
    //remember to update this on the launcher too

    public class ServerConfigFile
    {
        public string save_location = @"C:\Program Files (x86)\Steam\steamapps\common\ARK\ShooterGame\Saved\SavedArks\";
        public string save_map = @"Extinction.ark";
        public string ark_config;

        public ServerPermissionsRole base_permissions;
        public int permissions_version;
        public bool is_demo_server; //This will disable tribes and make everyone use the same tribe id
        public int demo_tribe_id; //The ID of the demo tribe to use.

        public ArkSlaveConnection connection;
    }

    public class ArkSlaveConnection
    {
        public string creds;
        public string id;
    }

    public class ServerPermissionsRole : List<string>
    {
        public bool CheckPermission(string name)
        {
            //If this key exists, the permission is OK
            return Contains(name);
        }
    }
}
