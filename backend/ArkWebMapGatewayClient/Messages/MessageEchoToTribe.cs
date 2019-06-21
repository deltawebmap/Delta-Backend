using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapGatewayClient.Messages
{
    public class MessageEchoToTribe : GatewayMessageBase
    {
        public string serverId;
        public int tribeId;
        public string msg;
    }
}
