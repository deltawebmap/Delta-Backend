using ArkWebMapGateway.Clients;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapGateway.ClientHandlers
{
    public class FrontendGatewayHandler : GatewayHandler
    {
        public FrontendGatewayConnection connection;

        public FrontendGatewayHandler(FrontendGatewayConnection conn)
        {
            this.connection = conn;
        }
    }
}
