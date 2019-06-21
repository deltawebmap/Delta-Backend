using ArkWebMapGatewayClient;
using ArkWebMapGatewayClient.Messages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapMasterServer
{
    public static class GatewayActionTool
    {
        public static void SendActionToTribe(GatewayMessageBase msg, int tribeId, string serverId)
        {
            //Set headers
            if (!msg.headers.ContainsKey("tribe_id"))
                msg.headers.Add("tribe_id", tribeId.ToString());
            if (!msg.headers.ContainsKey("server_id"))
                msg.headers.Add("server_id", serverId);

            //Send
            Program.gateway.SendMessage(new MessageEchoToTribe
            {
                msg = JsonConvert.SerializeObject(msg),
                serverId = serverId,
                tribeId = tribeId,
                opcode = GatewayMessageOpcode.EchoToTribe
            });
        }
    }
}
