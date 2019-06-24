using ArkWebMapLightspeedClient.Entities;
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
                List<byte> extendedBuffer = null;
                WebSocketReceiveResult result = await sock.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                OnOpen(e).GetAwaiter();
                while (!result.CloseStatus.HasValue)
                {
                    if(result.EndOfMessage)
                    {
                        //If the extended buffer was used, send that, or just respond with this
                        if(extendedBuffer == null)
                        {
                            byte[] msgBuffer = new byte[result.Count];
                            Array.Copy(buffer, msgBuffer, result.Count);
                            OnMsg(msgBuffer).GetAwaiter();
                        } else
                        {
                            //Copy this, then return the extended buffer
                            for (int i = 0; i < result.Count; i++)
                                extendedBuffer.Add(buffer[i]);
                            byte[] msgBuffer = extendedBuffer.ToArray();
                            extendedBuffer.Clear();
                            extendedBuffer = null;
                            OnMsg(msgBuffer).GetAwaiter();
                        }
                        
                    } else
                    {
                        //Write to extended buffer
                        if (extendedBuffer == null)
                            extendedBuffer = new List<byte>();
                        for (int i = 0; i < result.Count; i++)
                            extendedBuffer.Add(buffer[i]);
                    }

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
    }
}
