using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using ArkWebMapGatewayClient;
using ArkWebMapGatewayClient.Messages;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace ArkWebMapGateway
{
    public class GatewayConnection : WebsocketConnection
    {
        public override Task<bool> OnOpen(HttpContext e)
        {
            return Task.FromResult<bool>(true);
        }

        public override Task<bool> OnMsg(string msg)
        {
            throw new NotImplementedException();
        }

        public override Task<bool> OnClose(WebSocketCloseStatus? status)
        {
            return Task.FromResult<bool>(true);
        }

        public void OnSetHandler(GatewayHandler handler)
        {
            //Send set ID
            SendMsg(new MessageSetSessionID
            {
                opcode = GatewayMessageOpcode.SetSessionId,
                sessionId = handler.sessionId
            });
        }
    }
}
