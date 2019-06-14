using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace ArkWebMapLightspeed
{
    public class SubserverConnection : WebsocketConnection
    {
        public static async Task<SubserverConnection> AcceptConnection(Microsoft.AspNetCore.Http.HttpContext e)
        {
            //Authenticate this server. Request the master server.
            ServerValidationResponsePayload auth = await AuthenticateServer(e.Request.Query["id"], e.Request.Query["creds"]);
            if(auth == null)
            {
                await Program.QuickWriteToDoc(e, "Couldn't verify the identity of this server against the master server.", "text/plain", 500);
                return null;
            }

            //Create object
            SubserverConnection conn = new SubserverConnection(auth);

            //Accept
            await conn.Run(e, () =>
            {
                //Disconnect any other subservers with this ID (there shouldn't be any) and add us
                lock (ConnectionHolder.serverConnections)
                {
                    if (ConnectionHolder.serverConnections.ContainsKey(auth.server_id))
                    {
                        try
                        {
                            ConnectionHolder.serverConnections[auth.server_id].Close(new byte[0]).GetAwaiter();
                        }
                        catch { }
                        ConnectionHolder.serverConnections.Remove(auth.server_id, out SubserverConnection value);
                    }
                    ConnectionHolder.serverConnections.AddOrUpdate(auth.server_id, conn, (string s, SubserverConnection c) =>
                    {
                        throw new Exception("Unexpected error: SubServer is already registered!");
                    });
                }

                //TODO: Send off some notifications after this
            });
            return conn;
        }

        private static async Task<ServerValidationResponsePayload> AuthenticateServer(string id, string creds)
        {
            ServerValidationRequestPayload payload = new ServerValidationRequestPayload
            {
                server_creds = creds,
                server_id = id
            };

            try
            {
                //Make request
                HttpContent content = new StringContent(JsonConvert.SerializeObject(payload));
                HttpResponseMessage response;
                using (HttpClient wc = new HttpClient())
                    response = await wc.PostAsync("https://ark.romanport.com/api/server_validation", content);

                //Check
                if (!response.IsSuccessStatusCode)
                    return null;

                //Serialize response
                string responseString = await response.Content.ReadAsStringAsync();
                ServerValidationResponsePayload responsePayload = JsonConvert.DeserializeObject<ServerValidationResponsePayload>(responseString);
                if (responsePayload.server_id != id)
                    return null;
                return responsePayload;
            } catch
            {
                Console.WriteLine("Failed to authenticate server.");
                return null;
            }
        }

        public ServerValidationResponsePayload server;

        public SubserverConnection(ServerValidationResponsePayload server)
        {
            this.server = server;
        }

        public override Task OnMsg(byte[] msg)
        {
            SendMsg(msg);

            return Task.CompletedTask;
        }

        public override Task OnOpen(HttpContext e)
        {
            return Task.CompletedTask;
        }

        public override Task OnClose(WebSocketCloseStatus? status)
        {
            return Task.CompletedTask;
        }
    }
}
