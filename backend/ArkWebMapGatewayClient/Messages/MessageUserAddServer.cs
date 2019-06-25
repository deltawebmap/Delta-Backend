using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapGatewayClient.Messages
{
    public class MessageUserAddServer : GatewayMessageBase
    {
        public string userId;
        public string serverId;
    }
}
