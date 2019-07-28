using ArkBridgeSharedEntities.Entities.Master;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapGatewayClient.Messages
{
    public class MessageUpdateServer : GatewayMessageBase
    {
        public string server_id;
        public UsersMeReply_Server data;
    }
}
