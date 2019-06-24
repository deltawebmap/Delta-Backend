using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using ArkWebMapLightspeedClient.Entities;
using ArkWebMapMasterServer.NetEntities;
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
            try
            {
                //Read the token, status, and length of the response
                int token = BinaryIntEncoder.BytesToInt32(msg, 0);
                int status = BinaryIntEncoder.BytesToInt32(msg, 4);
                int bodyLength = BinaryIntEncoder.BytesToInt32(msg, 8);
                int headerLength = BinaryIntEncoder.BytesToInt32(msg, 12);

                //Read header
                byte[] header = new byte[headerLength];
                Array.Copy(msg, 16, header, 0, headerLength);

                //Read body
                byte[] body = new byte[bodyLength];
                Array.Copy(msg, 16 + headerLength, body, 0, bodyLength);

                //Get this from the pending requests
                if (!pendingRequests.TryGetValue(token, out PendingRequest request))
                    return Task.CompletedTask;

                //Respond
                request.e.Response.StatusCode = status;
                request.e.Response.ContentType = Encoding.UTF8.GetString(header);
                request.e.Response.Body.Write(body, 0, bodyLength);

                //Remove from pending requests
                pendingRequests.TryRemove(token, out request);

                return Task.CompletedTask;
            } catch (Exception ex)
            {
                Console.WriteLine("Failed to process incoming response: " + ex.Message + ex.StackTrace);
                return Task.CompletedTask;
            }
        }

        public override Task OnOpen(HttpContext e)
        {
            return Task.CompletedTask;
        }

        public override Task OnClose(WebSocketCloseStatus? status)
        {
            return Task.CompletedTask;
        }

        public int nextToken = 0;
        public ConcurrentDictionary<int, PendingRequest> pendingRequests = new ConcurrentDictionary<int, PendingRequest>();

        public async Task HandleHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, UsersMeReply user, string nextEndpoint)
        {
            //Create user
            MasterServerArkUser authUser = new MasterServerArkUser
            {
                id = user.id,
                is_steam_verified = true,
                profile_image_url = user.profile_image_url,
                screen_name = user.screen_name,
                servers = null,
                steam_id = user.steam_id
            };

            //Obtain token
            int token = nextToken++;

            //Create metadata
            RequestMetadata meta = new RequestMetadata
            {
                auth = authUser,
                endpoint = nextEndpoint,
                method = e.Request.Method,
                requestToken = token,
                version = 0,
                query = new Dictionary<string, string>()
            };
            foreach (var h in e.Request.Query)
                meta.query.Add(h.Key, h.Value);

            //Encode metadata as JSON and then encode the body (if sent)
            string metaString = JsonConvert.SerializeObject(meta);
            byte[] metaBytes = Encoding.UTF8.GetBytes(metaString);

            //Get the body
            byte[] body;
            if(e.Request.ContentLength.HasValue)
            {
                body = new byte[e.Request.ContentLength.Value];
                e.Request.Body.Read(body, 0, body.Length);
            } else
            {
                body = new byte[0];
            }

            //Encode the message content
            byte[] content = new byte[4 + metaBytes.Length + 4 + body.Length];
            BinaryIntEncoder.Int32ToBytes(metaBytes.Length).CopyTo(content, 0);
            metaBytes.CopyTo(content, 4);
            BinaryIntEncoder.Int32ToBytes(body.Length).CopyTo(content, metaBytes.Length + 4);
            body.CopyTo(content, metaBytes.Length + 8);

            //Insert to awaiting
            PendingRequest request = new PendingRequest
            {
                e = e,
                token = token
            };
            pendingRequests.TryAdd(token, request);

            //Now, send on the WebSocket
            SendMsg(content);

            //Stall
            while (pendingRequests.ContainsKey(token))
                await Task.Delay(10);
        }
    }
}
