using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using ArkWebMapGatewayClient.Sender;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace ArkWebMapGateway.Clients
{
    /// <summary>
    /// Used to send events. This is a write-only system.
    /// </summary>
    public class SenderConnection : WebsocketConnection
    {
        public static async Task<SenderConnection> HandleIncomingConnection(Microsoft.AspNetCore.Http.HttpContext e, string version)
        {
            //Do authentication
            if(e.Request.Query["auth_token"] != Program.config.admin_key)
            {
                await Program.QuickWriteToDoc(e, "Not Authenticated.", "text/plain", 401);
                return null;
            }

            //Start
            SenderConnection conn = new SenderConnection
            {
                
            };

            //Run
            await conn.Run(e, () =>
            {
                
            });

            return conn;
        }

        public override async Task<bool> OnClose(WebSocketCloseStatus? status)
        {
            //Do nothing
            return true;
        }

        public override async Task<bool> OnMsg(string msgText)
        {
            //Decode this as a message
            SenderMsg msg = JsonConvert.DeserializeObject<SenderMsg>(msgText);

            //Serialize payload
            string payload = null;
            if(msg.payload != null)
                payload = JsonConvert.SerializeObject(msg.payload);

            //Now, find all items that match this.
            GatewayConnection[] matches;
            lock(ClientHolder.connections)
            {
                matches = ClientHolder.connections.Where(x => x.CheckQuery(msg.query)).ToArray();
            }

            //Run action
            foreach(var m in matches)
            {
                if (msg.type == SenderMsgType.Relay)
                {
                    m.SendMsg(payload);
                }
                else if (msg.type == SenderMsgType.IdUpdate)
                {
                    await m.RefreshIndexes();
                }
            }

            return true;
        }

        public override async Task<bool> OnOpen(HttpContext e)
        {
            //Do nothing
            return true;
        }
    }
}
