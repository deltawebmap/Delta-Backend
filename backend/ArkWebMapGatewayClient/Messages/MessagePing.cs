using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapGatewayClient.Messages
{
    public class MessagePing : GatewayMessageBase
    {
        public string data { get; set; }
    }
}
