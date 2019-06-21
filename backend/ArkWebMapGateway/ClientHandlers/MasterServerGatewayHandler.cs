using ArkWebMapGateway.Clients;
using ArkWebMapGatewayClient.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapGateway.ClientHandlers
{
    public class MasterServerGatewayHandler : GatewayHandler
    {
        public MasterServerGatewayConnection connection;

        public MasterServerGatewayHandler(MasterServerGatewayConnection conn) : base()
        {
            this.connection = conn;
        }

        public override void Msg_EchoToTribe(MessageEchoToTribe data, object context)
        {
            MessageSender.SendMsgToTribe(data.msg, data.serverId, data.tribeId);
        }
    }
}
