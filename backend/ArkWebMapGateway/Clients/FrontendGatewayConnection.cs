using ArkWebMapGateway.Entities;
using ArkWebMapGatewayClient;
using ArkWebMapMasterServer.NetEntities;
using LibDeltaSystem.Db.System;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapGateway.Clients
{
    public class FrontendGatewayConnection : GatewayConnection
    {
        public DbUser user;

        public static async Task<FrontendGatewayConnection> HandleIncomingConnection(Microsoft.AspNetCore.Http.HttpContext e, string version)
        {
            //Do authentication
            DbUser user = await Program.conn.AuthenticateUserToken(e.Request.Query["auth_token"]);
            if(user == null)
            {
                await Program.QuickWriteToDoc(e, "Not Authenticated.", "text/plain", 401);
                return null;
            }

            //Start
            FrontendGatewayConnection conn = new FrontendGatewayConnection
            {
                type = "USER",
                id = user.id,
                user = user,
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
            //Fetch owned servers
            var ownedServers = await user.GetGameServersAsync();

            //Now, add them
            lock(indexes)
            {
                indexes.Clear();
                indexes.Add("SERVERS", new List<string>());
                indexes.Add("TRIBES", new List<string>());

                foreach(var s in ownedServers)
                {
                    indexes["SERVERS"].Add(s.Item1.id);
                    indexes["TRIBES"].Add(s.Item1.id+"/"+s.Item2.tribe_id.ToString());
                }
            }
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
