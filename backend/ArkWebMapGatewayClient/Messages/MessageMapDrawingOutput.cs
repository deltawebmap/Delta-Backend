using ArkWebMapGatewayClient.Messages.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapGatewayClient.Messages
{
    public class MessageMapDrawingOutput : GatewayMessageBase
    {
        public List<ArkTribeMapDrawingPoint> points;
        public int mapId;
        public string senderSessionId;
    }
}
