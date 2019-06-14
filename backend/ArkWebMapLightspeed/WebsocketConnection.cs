using ArkWebMapMasterServer.NetEntities;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ArkWebMapLightspeed
{
    public delegate void OnWebsocketCreatedCallback();

    public abstract class WebsocketConnection
    {
        public abstract Task OnMsg(byte[] msg);
        public abstract Task OnOpen(Microsoft.AspNetCore.Http.HttpContext e);
        public abstract Task OnClose(WebSocketCloseStatus? status);

        public WebSocket sock;
        private Queue<byte[]> sendQueue;
        private Task bgTask;

        public WebsocketConnection()
        {
            sendQueue = new Queue<byte[]>();
        }

        public async Task Run(Microsoft.AspNetCore.Http.HttpContext e, OnWebsocketCreatedCallback readyCallback)
        {
            //Accept WebSocket
            WebSocket wc = await e.WebSockets.AcceptWebSocketAsync();
            sock = wc;
            readyCallback();
            bgTask = SenderLoop();
            try
            {
                byte[] buffer = new byte[1024 * 4];
                WebSocketReceiveResult result = await sock.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                OnOpen(e).GetAwaiter();
                while (!result.CloseStatus.HasValue)
                {
                    //Read buffer and call handler
                    byte[] msgBuffer = new byte[result.Count];
                    Array.Copy(buffer, msgBuffer, result.Count);
                    OnMsg(msgBuffer).GetAwaiter();

                    //Get next result
                    result = await sock.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                }
                await sock.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
                await OnClose(result.CloseStatus);
            }
            catch (Exception ex)
            {
                await OnClose(null);
            }
        }

        public async Task Close(byte[] reason)
        {
            await sock.SendAsync(new ArraySegment<byte>(reason), WebSocketMessageType.Close, true, CancellationToken.None);
        }

        public void SendMsg(byte[] msg)
        {
            lock (sendQueue)
                sendQueue.Enqueue(msg);
        }

        private async Task SenderLoop()
        {
            byte[] buffer;
            while (true)
            {
                if(sendQueue.TryDequeue(out buffer))
                {
                    await sock.SendAsync(new ArraySegment<byte>(buffer, 0, buffer.Length), WebSocketMessageType.Binary, true, CancellationToken.None);
                } else
                {
                    await Task.Delay(5);
                }
            }
        }

        //

        public async Task HandleIncomingRequest(Microsoft.AspNetCore.Http.HttpContext e, string next, UsersMeReply user)
        {
            //First, convert this to the MasterServerArkUser format the client expects.
            ArkHttpServer.Entities.MasterServerArkUser c = new ArkHttpServer.Entities.MasterServerArkUser
            {
                id = user.id,
                steam_id = user.steam_id,
                is_steam_verified = true,
                profile_image_url = user.profile_image_url,
                screen_name = user.screen_name,
                servers = new List<string>()
            };
            foreach (var s in user.servers)
                c.servers.Add(s.id);

            //Now, create a session and token.
            //TODO
        }
    }
}
