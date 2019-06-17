using ArkWebMapGateway.ClientHandlers;
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
    public class MasterServerGatewayConnection : GatewayConnection
    {
        public MasterServerGatewayHandler handler;

        public static async Task<MasterServerGatewayConnection> HandleIncomingConnection(Microsoft.AspNetCore.Http.HttpContext e, string version)
        {
            //Ensure this really is the master server
            bool authenticated;
            try
            {
                using (WebClient wc = new WebClient())
                {
                    string s = wc.DownloadString("https://ark.romanport.com/api/system_server_validation?type=master&value="+System.Web.HttpUtility.UrlEncode(e.Request.Query["auth_token"]));
                    authenticated = JsonConvert.DeserializeObject<ArkBridgeSharedEntities.Entities.TrueFalseReply>(s).ok;
                }
            }
            catch (Exception ex)
            {
                authenticated = false;
            }
            if(!authenticated)
            {
                //Send auth failed.
                Console.WriteLine("Rejecting connection from master server because it failed to identify itself.");
                await Program.QuickWriteToDoc(e, "Not Authenticated.", "text/plain", 401);
                return null;
            }

            //Start
            MasterServerGatewayConnection conn = new MasterServerGatewayConnection();
            conn.handler = new MasterServerGatewayHandler(conn);
            await conn.Run(e, () =>
            {
                //Attach
                if (ConnectionHolder.master == null)
                {
                    ConnectionHolder.master = conn;
                } else
                {
                    lock (ConnectionHolder.master)
                    {
                        ConnectionHolder.master.DisconectOld().GetAwaiter().GetResult();
                        ConnectionHolder.master = conn;
                    }
                }
                Console.WriteLine("Attached.");
            });

            return conn;
        }

        /// <summary>
        /// Disconnects an old master server if it was running for some reason.
        /// </summary>
        /// <returns></returns>
        public async Task DisconectOld()
        {
            await Close(new byte[0]);
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

        public override Task<bool> OnMsg(string msg)
        {
            //Deserialize as base type to get the opcode
            GatewayMessageBase b = JsonConvert.DeserializeObject<GatewayMessageBase>(msg);

            //Now, let it be handled like normal.
            handler.HandleMsg(b.opcode, msg, this);

            //Return OK
            return Task.FromResult<bool>(true);
        }
    }
}
