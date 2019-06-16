using ArkWebMapGateway.Clients;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapGateway.ClientHandlers
{
    public class MasterServerGatewayHandler : GatewayHandler
    {
        public MasterServerGatewayConnection connection;

        public MasterServerGatewayHandler(MasterServerGatewayConnection conn)
        {
            this.connection = conn;
        }
    }
}
