using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapMasterServer
{
    public class MasterServerConfig
    {
        public string steam_api_key;
        public string firebase_cloud_messages_api_key;
        public string database_pathname;
        public Dictionary<string, string> system_server_keys; //Stores keys for each system service running on the network. Should be secure.
    }
}
