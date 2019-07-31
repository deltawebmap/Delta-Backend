using ArkBridgeSharedEntities.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkHttpServer.Entities
{
    public class CachedSteamUser
    {
        public string _id { get; set; }
        public SteamProfile payload { get; set; }
        public long expire_time { get; set; }
    }
}
