using ArkBridgeSharedEntities.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapGatewayClient.Messages
{
    public class SendPushNotificationToTribe : GatewayMessageBase
    {
        public string serverId;
        public int tribeId;
        public ArkV2Notification payload;
    }
}
