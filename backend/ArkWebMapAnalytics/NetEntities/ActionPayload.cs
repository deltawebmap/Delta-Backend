using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapAnalytics.NetEntities
{
    public class ActionPayload : BasePayload
    {
        public string topic;
        public string server_id;
        public bool server_online;
        public Dictionary<string, string> extras;
    }
}
