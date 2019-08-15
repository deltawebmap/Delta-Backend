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
        public int? demo_tribe_id; //The ID of the demo tribe to use.
        public Dictionary<string, string> demo_renames; //Renames dinosaurs with the key name to the value name.

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
