using ArkWebMapGatewayClient.Messages.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapGatewayClient.Messages
{
    public class MessageMapDrawingInput : GatewayMessageBase
    {
        public List<ArkTribeMapDrawingPoint> points;
        public int mapId;
    }
}
