using ArkBridgeSharedEntities.Entities;
using ArkWebMapGatewayClient.Messages;
using ArkWebMapMasterServer.PresistEntities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Bridge
{
    public static class V2ServerNotificationRequest
    {
        public static Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, ArkServer s)
        {
            //Decode the notification from the body
            ArkV2NotificationRequest request = Program.DecodePostBody<ArkV2NotificationRequest>(e);

            //Set the appropriate values
            ArkV2Notification payload = request.payload;
            if (payload == null)
                return Program.QuickWriteStatusToDoc(e, false);
            payload.serverName = s.display_name;

            //Send to the GATEWAY
            Program.gateway.SendMessage(new SendPushNotificationToTribe
            {
                opcode = ArkWebMapGatewayClient.GatewayMessageOpcode.SendPushNotificationToTribe,
                serverId = s._id,
                tribeId = request.targetTribeId,
                payload = payload
            });
            return Program.QuickWriteStatusToDoc(e, true);
        }
    }
}
