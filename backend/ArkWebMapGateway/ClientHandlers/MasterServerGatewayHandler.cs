using ArkWebMapGateway.Clients;
using ArkWebMapGatewayClient.Messages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArkWebMapGateway.ClientHandlers
{
    public class MasterServerGatewayHandler : GatewayHandler
    {
        public MasterServerGatewayConnection connection;
        public static int latestNotificationId = 0;

        public MasterServerGatewayHandler(MasterServerGatewayConnection conn) : base()
        {
            this.connection = conn;
        }

        public override void Msg_EchoToTribe(MessageEchoToTribe data, object context)
        {
            MessageSender.SendMsgToTribe(data.msg, data.serverId, data.tribeId);
        }

        public override void Msg_SendPushNotificationToTribe(SendPushNotificationToTribe data, object context)
        {
            data.payload.uuid = latestNotificationId++;
            string message = JsonConvert.SerializeObject(data.payload);
            int targetTribeId = data.tribeId;

            //Find clients to send to and send them the message
            var clients = ConnectionHolder.notificationClients.Where(x => x.serverIds.ContainsKey(data.serverId));
            foreach (var c in clients)
            {
                int clientTribeId = c.serverIds[data.serverId];
                if (clientTribeId == targetTribeId)
                {
                    c.SendMsg(message);
                }
            }
        }
    }
}
