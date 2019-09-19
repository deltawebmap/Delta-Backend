using ArkBridgeSharedEntities.Entities;
using ArkWebMapMasterServer.PresistEntities;
using LibDeltaSystem.Db.System;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapMasterServer.NetEntities
{
    public class ArkSetupProxySession
    {
        public List<ArkSetupProxyMessage> toWeb = new List<ArkSetupProxyMessage>(); //User interface
        public List<ArkSetupProxyMessage> toServer = new List<ArkSetupProxyMessage>(); //To the server to be set up

        public DbUser user;
        public DbServer server;

        public bool up;
        public bool claimed;
    }
}
