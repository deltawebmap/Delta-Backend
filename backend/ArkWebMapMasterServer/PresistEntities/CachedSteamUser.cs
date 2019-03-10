using ArkBridgeSharedEntities.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapMasterServer.PresistEntities
{
    public class CachedSteamUser
    {
        public string _id { get; set; } //SteamID
        public SteamProfile payload { get; set; } //The actual data
        public long cacheDate { get; set; } //Time of this request. Expires after some amount of time.
    }
}
