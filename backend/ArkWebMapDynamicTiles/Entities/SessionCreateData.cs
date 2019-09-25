using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapDynamicTiles.Entities
{
    public class SessionCreateData
    {
        public string token;
        public int heartbeat_policy_ms;

        public string url_map;
        public string url_heartbeat;
    }
}
