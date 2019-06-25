using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapGatewayClient.Messages
{
    public class MessageServerStateChange : GatewayMessageBase
    {
        public bool isUp;
        public string serverId;
    }
}
