using ArkWebMapGatewayClient;
using ArkWebMapGatewayClient.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapLightspeed
{
    public class GatewayHandler : GatewayMessageHandler
    {
        public override void Msg_UserAddServer(MessageUserAddServer data, object context)
        {
            ConnectionHolder.DevalidateUserToken(data.userId);
        }

        public override void Msg_UserLogOut(MessageUserLogOut data, object context)
        {
            ConnectionHolder.DevalidateUserToken(data.userId);
        }
    }
}
