using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using ArkWebMapGatewayClient;
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
            //Deserialize as base type to get the opcode
            GatewayMessageBase b = JsonConvert.DeserializeObject<GatewayMessageBase>(msg);

            //Now, let it be handled like normal.
            handler.HandleMsg(b.opcode, msg, this);

            //Return OK
            return Task.FromResult<bool>(true);
        }

        public override Task<bool> OnClose(WebSocketCloseStatus? status)
        {
            return Task.FromResult<bool>(true);
        }
    }
}
