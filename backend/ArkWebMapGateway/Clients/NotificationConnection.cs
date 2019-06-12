using System;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using ArkWebMapMasterServer.NetEntities;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace ArkWebMapGateway.Clients
{
    /// <summary>
    /// Not techincally used in the asme way as normal Gateway connections. However, it is still used as a WebSocket connection for notifications
    /// </summary>
    public class NotificationConnection : WebsocketConnection
    {
        public Dictionary<string, int> serverIds = new Dictionary<string, int>(); //Server ID, tribe ID

        public static async Task<NotificationConnection> HandleIncomingConnection(Microsoft.AspNetCore.Http.HttpContext e, string version)
        {
            //Authenticate
            UsersMeReply user;
            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.Headers.Add("Authorization", "Bearer " + e.Request.Query["auth_token"]);
                    string s = wc.DownloadString("https://ark.romanport.com/api/users/@me/");
                    user = JsonConvert.DeserializeObject<UsersMeReply>(s);
                }
            }
            catch (Exception ex)
            {
                //Send auth failed.
                await Program.QuickWriteToDoc(e, "Not Authenticated.", "text/plain", 401);
                return null;
            }

            //Create 
            NotificationConnection client = new NotificationConnection();

            //Record all Server IDs and tribe IDs
            foreach (var server in user.servers)
            {
                if (server.has_ever_gone_online && !server.is_hidden)
                {
                    client.serverIds.Add(server.id, server.tribeId);
                    ServerNameCache.UpdateOrInsertServer(server.id, server.display_name);
                }
            }

            //Insert this into the binder
            lock (ConnectionHolder.notificationClients)
                ConnectionHolder.notificationClients.Add(client);

            //Run
            await client.Run(e, () => {

            });

            return client;
        }

        public override Task<bool> OnMsg(string msg)
        {
            return Task.FromResult(true);
        }

        public override Task<bool> OnOpen(HttpContext e)
        {
            return Task.FromResult(true);
        }

        public override Task<bool> OnClose(WebSocketCloseStatus? status)
        {
            //Unbind
            lock (ConnectionHolder.notificationClients)
                ConnectionHolder.notificationClients.Remove(this);

            return Task.FromResult(true);
        }
    }
}
