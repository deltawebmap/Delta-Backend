using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

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
            Console.WriteLine(msg);
            return Task.FromResult<bool>(true);
        }

        public override Task<bool> OnClose(WebSocketCloseStatus? status)
        {
            return Task.FromResult<bool>(true);
        }
    }
}
