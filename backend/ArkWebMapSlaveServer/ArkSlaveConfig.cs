using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapSlaveServer
{
    public class ArkSlaveConfig
    {
        public int web_port;
        public ArkHttpServer.ServerConfigFile child_config;
        public ArkSlaveConnection auth;
        public bool debug_mode; //Enables additional logging and disables the security check when HTTP requests come in.
    }

    public class ArkSlaveConnection
    {
        public string creds;
        public string id;
    }
}
