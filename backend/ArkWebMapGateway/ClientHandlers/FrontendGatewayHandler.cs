using ArkWebMapGateway.Clients;
using ArkWebMapGateway.Entities;
using ArkWebMapGatewayClient;
using ArkWebMapGatewayClient.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapGateway.ClientHandlers
{
    public class FrontendGatewayHandler : GatewayHandler
    {
        public FrontendGatewayConnection connection;

        public FrontendGatewayHandler(FrontendGatewayConnection conn) : base()
        {
            this.connection = conn;
        }

        private GatewayFrontendMsgMeta GetMeta(object c)
        {
            return (GatewayFrontendMsgMeta)c;
        }

        public override void Msg_TribeMapInput(MessageMapDrawingInput data, object context)
        {
            GatewayFrontendMsgMeta meta = GetMeta(context);

            //Echo back to tribemates
            MessageMapDrawingOutput e = new MessageMapDrawingOutput
            {
                mapId = data.mapId,
                points = data.points,
                opcode = GatewayMessageOpcode.TribeMapFrontendOutput,
                senderSessionId = sessionId
            };
            MessageSender.SendMsgToTribe(e, meta.server_id, meta.tribe_id);
        }
    }
}
