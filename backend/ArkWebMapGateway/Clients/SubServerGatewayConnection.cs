using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapGateway.Clients
{
    public class SubServerGatewayConnection : GatewayConnection
    {


        public static async Task<SubServerGatewayConnection> HandleIncomingConnection(Microsoft.AspNetCore.Http.HttpContext e, string version)
        {
            //Start
            SubServerGatewayConnection conn = new SubServerGatewayConnection();
            await conn.Run(e, () =>
            {
                //Ready
            });

            return conn;
        }
    }
}
