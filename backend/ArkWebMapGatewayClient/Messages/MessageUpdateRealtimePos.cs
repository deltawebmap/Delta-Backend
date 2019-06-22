using ArkWebMapGatewayClient.Messages.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapGatewayClient.Messages
{
    public class MessageUpdateRealtimePos : GatewayMessageBase
    {
        public List<UpdateEntityRealtimePosition> updates;
    }
}
