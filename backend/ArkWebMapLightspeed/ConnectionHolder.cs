using ArkWebMapMasterServer.NetEntities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapLightspeed
{
    public class ConnectionHolder
    {
        public static ConcurrentDictionary<string, SubserverConnection> serverConnections = new ConcurrentDictionary<string, SubserverConnection>();
        public static ConcurrentDictionary<string, UsersMeReply> cachedTokens = new ConcurrentDictionary<string, UsersMeReply>();
    }
}
