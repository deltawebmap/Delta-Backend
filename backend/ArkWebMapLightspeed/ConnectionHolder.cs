using ArkWebMapMasterServer.NetEntities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArkWebMapLightspeed
{
    public class ConnectionHolder
    {
        public static ConcurrentDictionary<string, SubserverConnection> serverConnections = new ConcurrentDictionary<string, SubserverConnection>();
        public static ConcurrentDictionary<string, UsersMeReply> cachedTokens = new ConcurrentDictionary<string, UsersMeReply>();

        public static void DevalidateUserToken(string userId)
        {
            lock(cachedTokens)
            {
                var keys = cachedTokens.Keys;
                foreach(var k in keys)
                {
                    if (cachedTokens[k].id == userId)
                        cachedTokens.TryRemove(k, out UsersMeReply value);
                }
            }
        }
    }
}
