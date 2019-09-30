using ArkWebMapGatewayClient;
using LibDeltaSystem.Db.System;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapGateway.Clients
{
    public class SubserverGatewayConnection : GatewayConnection
    {
        public DbMachine server;

        public static async Task<SubserverGatewayConnection> HandleIncomingConnection(Microsoft.AspNetCore.Http.HttpContext e, string version)
        {
            //Do authentication
            DbMachine server = await Program.conn.AuthenticateMachineTokenAsync(e.Request.Query["auth_token"]);
            if (server == null)
            {
                await Program.QuickWriteToDoc(e, "Not Authenticated.", "text/plain", 401);
                return null;
            }

            //Start
            SubserverGatewayConnection conn = new SubserverGatewayConnection
            {
                type = "SUBSERVER",
                id = server.id,
                server = server,
                indexes = new Dictionary<string, List<string>>()
            };

            //Refresh indexes
            await conn.RefreshIndexes();

            //Run
            await conn.Run(e, () =>
            {
                //Ready
                //Add
                lock (ClientHolder.connections)
                    ClientHolder.connections.Add(conn);
            });

            return conn;
        }

        /// <summary>
        /// Refreshes the indexes so we can send messages.
        /// </summary>
        /// <returns></returns>
        public override async Task RefreshIndexes()
        {
            //No indexes as of now...
        }

        public override Task<bool> OnClose(WebSocketCloseStatus? status)
        {
            //Remove this from the list of clients
            lock (ClientHolder.connections)
                ClientHolder.connections.Remove(this);

            return base.OnClose(status);
        }

        public override async Task<bool> OnMsg(string msg)
        {
            //Ignore
            return true;
        }
    }
}
