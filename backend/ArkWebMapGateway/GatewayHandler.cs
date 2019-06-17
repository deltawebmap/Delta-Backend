using ArkWebMapGatewayClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapGateway
{
    public class GatewayHandler : GatewayMessageHandler
    {
        public string sessionId;

        public GatewayHandler()
        {
            //Generate a random session id
            sessionId = Program.GenerateRandomString(24);
        }
    }
}
