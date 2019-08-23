using ArkBridgeSharedEntities.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapMasterServer.NetEntities
{
    public class SteamValidationResponse
    {
        public bool ok;
        public string next;
        public string steam_id;
        public SteamProfile profile;
    }
}
