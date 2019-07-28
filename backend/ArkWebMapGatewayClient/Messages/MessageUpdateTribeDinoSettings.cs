using ArkBridgeSharedEntities.Entities.Master;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapGatewayClient.Messages
{
    public class MessageUpdateTribeDinoSettings : GatewayMessageBase
    {
        public DinoTribeSettings data;
    }
}
