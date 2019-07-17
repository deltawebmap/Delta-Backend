using ArkBridgeSharedEntities.Entities;
using ArkBridgeSharedEntities.Entities.BasicTribeLog;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapMasterServer.NetEntities
{
    public class SingleServerHub
    {
        public BasicTribeLogEntry[] log;
        public Dictionary<string, SteamProfile> profiles;
    }
}
