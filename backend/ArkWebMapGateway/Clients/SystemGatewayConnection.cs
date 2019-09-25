using ArkWebMapGatewayClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapGateway.Clients
{
    /*public class SystemGatewayConnection : GatewayConnection
    {
        public SystemGatewayHandler handler;

        public static async Task<SystemGatewayConnection> HandleIncomingConnection(Microsoft.AspNetCore.Http.HttpContext e, string version)
        {
            //Ensure this really is the master server
            bool authenticated;
            try
            {
                using (WebClient wc = new WebClient())
                {
                    string s = wc.DownloadString("https://deltamap.net/api/system_server_validation?type=system&value=" + System.Web.HttpUtility.UrlEncode(e.Request.Query["auth_token"]));
                    authenticated = JsonConvert.DeserializeObject<ArkBridgeSharedEntities.Entities.TrueFalseReply>(s).ok;
                }
            }
            catch (Exception ex)
            {
                authenticated = false;
            }
            if (!authenticated)
            {
                //Send auth failed.
                Console.WriteLine("Rejecting connection from system server because it failed to identify itself.");
                await Program.QuickWriteToDoc(e, "Not Authenticated.", "text/plain", 401);
                return null;
            }

            //Start
            SystemGatewayConnection conn = new SystemGatewayConnection();
            conn.handler = new SystemGatewayHandler(conn);
            conn.OnSetHandler(conn.handler);
            await conn.Run(e, () =>
            {
                lock (ConnectionHolder.systemClients)
                    ConnectionHolder.systemClients.Add(conn);
            });

            return conn;
        }

        public override Task<bool> OnClose(WebSocketCloseStatus? status)
        {
            //Remove this from the list of clients
            lock (ConnectionHolder.systemClients)
                ConnectionHolder.systemClients.Remove(this);

            return Task.FromResult<bool>(true);
        }

        public override Task<bool> OnMsg(string msg)
        {
            //Deserialize as base type to get the opcode
            GatewayMessageBase b = JsonConvert.DeserializeObject<GatewayMessageBase>(msg);

            //Now, let it be handled like normal.
            handler.HandleMsg(b.opcode, msg, this);

            //Return OK
            return Task.FromResult<bool>(true);
        }
    }*/
}
