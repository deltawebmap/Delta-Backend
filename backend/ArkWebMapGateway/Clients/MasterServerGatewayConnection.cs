using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapGateway.Clients
{
    public class MasterServerGatewayConnection : GatewayConnection
    {
        public static async Task<MasterServerGatewayConnection> HandleIncomingConnection(Microsoft.AspNetCore.Http.HttpContext e, string version)
        {
            //TODO: Add authentication

            //Start
            MasterServerGatewayConnection conn = new MasterServerGatewayConnection();
            await conn.Run(e, () =>
            {

            });

            return conn;
        }

        /// <summary>
        /// Disconnects an old master server if it was running for some reason.
        /// </summary>
        /// <returns></returns>
        public static async Task DisconectOld()
        {

        }

        public override Task<bool> OnClose(WebSocketCloseStatus? status)
        {
            lock(ConnectionHolder.master)
            {
                //Remove this client from the binder
                if (ConnectionHolder.master == this)
                    ConnectionHolder.master = null;
            }

            return Task.FromResult<bool>(true);
        }
    }
}
