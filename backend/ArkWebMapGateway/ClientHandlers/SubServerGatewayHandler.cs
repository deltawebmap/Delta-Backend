using ArkWebMapGateway.Clients;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapGateway.ClientHandlers
{
    public class SubServerGatewayHandler : GatewayHandler
    {
        public SubServerGatewayConnection connection;

        public SubServerGatewayHandler(SubServerGatewayConnection conn)
        {
            this.connection = conn;
        }
    }
}
