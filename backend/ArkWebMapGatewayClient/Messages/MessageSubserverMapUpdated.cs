using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapGatewayClient.Messages
{
    public class MessageSubserverMapUpdated : GatewayMessageBase
    {
        public DateTime save_time;
        public float game_time;
    }
}
