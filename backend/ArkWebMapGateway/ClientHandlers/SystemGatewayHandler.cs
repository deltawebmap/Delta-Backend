using ArkWebMapGateway.Clients;
using ArkWebMapGatewayClient.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapGateway.ClientHandlers
{
    public class SystemGatewayHandler : GatewayHandler
    {
        public SystemGatewayConnection connection;

        public SystemGatewayHandler(SystemGatewayConnection conn) : base()
        {
            this.connection = conn;
        }

        public override void Msg_MessageServerStateChange(MessageServerStateChange data, object context)
        {
            //Echo to master server and users that might be connected
            MessageSender.SendMsgToMasterServer(data);
            MessageSender.SendMsgToServerMembers(data, data.serverId);
        }

        public override void Msg_SubserverOfflineDataUpdated(MessageSubserverOfflineDataUpdated data, object context)
        {
            //Echo to master server and users that might be connected
            MessageSender.SendMsgToMasterServer(data);
            MessageSender.SendMsgToServerMembers(data, data.server_id);
        }
    }
}
