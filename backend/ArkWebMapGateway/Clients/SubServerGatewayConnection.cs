using ArkWebMapGatewayClient;
using Newtonsoft.Json;
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
            //Set handler
            //conn.OnSetHandler(conn.handler);
            await conn.Run(e, () =>
            {
                //Ready
            });

            return conn;
        }

        public override Task<bool> OnMsg(string msg)
        {
            //Deserialize as base type to get the opcode
            GatewayMessageBase b = JsonConvert.DeserializeObject<GatewayMessageBase>(msg);

            //Now, let it be handled like normal.
            //handler.HandleMsg(b.opcode, msg, this); //TODO

            //Return OK
            return Task.FromResult<bool>(true);
        }
    }
}
