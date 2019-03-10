using ArkBridgeSharedEntities.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapMasterServer.NetEntities
{
    

    public class SteamProfile_Players
    {
        public List<SteamProfile> players;
    }

    public class SteamProfile_Full
    {
        public SteamProfile_Players response;
    }
}
