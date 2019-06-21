using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapGatewayClient.Messages
{
    public class MessageSetSessionID : GatewayMessageBase
    {
        public string sessionId { get; set; }
    }
}
