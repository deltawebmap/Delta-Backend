using ArkWebMapLightspeedClient.Entities;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace ArkWebMapLightspeedClient
{
    public delegate Task LightspeedHandler(LightspeedRequest request);
    public class AWMLightspeedClient
    {
        public const string REMOTE_CONFIG_ENDPOINT = "https://config.deltamap.net/prod/lightspeed_config.json";

        public bool logging_enabled;
        public string clientId;
        public string clientToken;
        public int clientGame;
        public LightspeedHandler handler;

        public static LightspeedConfigFile config;

        private ClientWebSocket sock;
        private Thread rxThread;
        private Thread txThread;

        private Queue<byte[]> txQueue;
        private System.Timers.Timer retryTimer;

        public bool is_connected;

        /// <summary>
        /// Creates and configures the client.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="client_name"></param>
        /// <param name="client_name_extra"></param>
        /// <param name="client_version_major"></param>
        /// <param name="client_version_minor"></param>
        /// <returns></returns>
        public static AWMLightspeedClient CreateClient(string clientId, string clientToken, int clientGame, LightspeedHandler handler, bool logging_enabled)
        {
            //Create object and add args
            AWMLightspeedClient client = new AWMLightspeedClient
            {
                clientId = clientId,
                clientToken = clientToken,
                clientGame = clientGame,
                logging_enabled = logging_enabled,
                handler = handler,
                txQueue = new Queue<byte[]>(),
            };

            //Get config file
            GetConfigFile();

            //Try to make a connection.
            client.TryMakeConnection().GetAwaiter().GetResult();

            //Create threads
            client.rxThread = new Thread(client.RxBgThread);
            client.rxThread.IsBackground = true;
            client.rxThread.Start();

            client.txThread = new Thread(client.TxBgThread);
            client.txThread.IsBackground = true;
            client.txThread.Start();

            return client;
        }

        public static LightspeedConfigFile GetConfigFile()
        {
            if (config != null)
                return config;

            //Make a request to the client config endpoint to download the remote configuration file
            string configText;
            try
            {
                using (WebClient wc = new WebClient())
                    configText = wc.DownloadString(REMOTE_CONFIG_ENDPOINT);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to download remote LIGHTSPEED configuration file.");
            }
            config = JsonConvert.DeserializeObject<LightspeedConfigFile>(configText);

            return config;
        }

        public void LogMsg(string topic, string msg)
        {
            if (logging_enabled)
                Console.WriteLine($"[AWM GATEWAY Client / {topic}] {msg}");
        }

        /// <summary>
        /// Queues a message
        /// </summary>
        public void SendMessage(byte[] msg)
        {
            txQueue.Enqueue(msg);
        }

        /// <summary>
        /// Tries to make a connection to the WebSocket. If it fails, it starts the retry timer.
        /// </summary>
        /// <returns></returns>
        private async Task TryMakeConnection()
        {
            //Try and make a connection.
            LogMsg("TryMakeConnection", "Attempting connection to LIGHTSPEED.");
            LogMsg("TryMakeConnection", "LIGHTSPEED connection params: " + CreateGatewayURI().ToString());
            try
            {
                await CreateConnection();
                LogMsg("TryMakeConnection", "Successfully created connection to GATEWAY.");

                //Ok. Stop retry timer.
                if (retryTimer != null)
                    retryTimer.Stop();
                retryTimer = null;
            }
            catch (Exception ex)
            {
                //Failed. If we haven't started the retry timer, try
                LogMsg("TryMakeConnection", "Connection to GATEWAY failed.");
                if (retryTimer == null)
                {
                    retryTimer = new System.Timers.Timer();
                    retryTimer.AutoReset = true;
                    retryTimer.Interval = config.reconnect_delay_seconds * 1000;
                    retryTimer.Elapsed += RetryTimer_Elapsed;
                    retryTimer.Start();
                }
            }
        }

        private void RetryTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            TryMakeConnection().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Creates the URI to connect to.
        /// </summary>
        /// <returns></returns>
        private Uri CreateGatewayURI()
        {
            Uri uri = new Uri(config.server_endpoint.Replace("{clientId}", WebUtility.UrlEncode(clientId)).Replace("{clientToken}", WebUtility.UrlEncode(clientToken)).Replace("{clientGame}", clientGame.ToString()));
            return uri;
        }

        private async Task CreateConnection()
        {
            //Create a connection to the GATEWAY
            //Create the endpoint to connect to
            Uri uri = CreateGatewayURI();

            //Open WebSocket connection to here
            ClientWebSocket wc = new ClientWebSocket();
            await wc.ConnectAsync(uri, System.Threading.CancellationToken.None);
            sock = wc;
            is_connected = true;
        }

        private void RxBgThread()
        {
            while (true)
            {
                while (!is_connected)
                    Thread.Sleep(2);
                try
                {
                    byte[] buf = new byte[config.buffer_size];
                    WebSocketReceiveResult ar = sock.ReceiveAsync(new ArraySegment<byte>(buf), CancellationToken.None).GetAwaiter().GetResult();
                    if (ar.MessageType == WebSocketMessageType.Close)
                        OnShutdown();
                    else if (ar.MessageType == WebSocketMessageType.Text)
                    {
                        //Binary messages are not supported. Drop.
                        LogMsg("RxBgThread", "Text messages are not supported. Are you sure we're talking to the right server?");
                    }
                    else if (ar.MessageType == WebSocketMessageType.Binary)
                    {
                        //Expected this. Handle.
                        byte[] bufInput = new byte[ar.Count];
                        Array.Copy(buf, bufInput, ar.Count);
                        OnMsgRx(bufInput);
                    }
                }
                catch (Exception ex)
                {
                    LogMsg("RxBgThread", "Unexpected fatal error, dropping: " + ex.Message + ex.StackTrace);
                    OnShutdown();
                }
            }
        }

        private void TxBgThread()
        {
            while (true)
            {
                while (!is_connected)
                    Thread.Sleep(2);
                if (txQueue.TryDequeue(out byte[] msg))
                {
                    try
                    {
                        sock.SendAsync(new ArraySegment<byte>(msg), WebSocketMessageType.Binary, true, CancellationToken.None);
                    }
                    catch (Exception ex)
                    {
                        LogMsg("TxBgThread", "Unexpected fatal error, dropping: " + ex.Message);
                        OnShutdown();
                    }
                }
                else
                {
                    Thread.Sleep(2);
                }
            }
        }

        /// <summary>
        /// Called when a connection is lost.
        /// </summary>
        private void OnShutdown()
        {
            is_connected = false;

            //Log
            LogMsg("OnShutdown", "Connection to GATEWAY lost. Trying to reconnect...");

            //Wait and try to retry
            Thread.Sleep(5000);
            TryMakeConnection();
        }

        private void OnMsgRx(byte[] msg)
        {
            //Log
            LogMsg("OnMsgRx", "Got incoming message.");

            //Read the data
            int headerLength = BinaryIntEncoder.BytesToInt32(msg, 0);
            int bodyLength = BinaryIntEncoder.BytesToInt32(msg, 4 + headerLength);
            byte[] headerBytes = new byte[headerLength];
            byte[] bodyBytes = new byte[bodyLength];
            Array.Copy(msg, 4, headerBytes, 0, headerLength);
            Array.Copy(msg, 4 + headerLength + 4, bodyBytes, 0, bodyLength);

            //Deserialize header
            RequestMetadata metadata = JsonConvert.DeserializeObject<RequestMetadata>(Encoding.UTF8.GetString(headerBytes));

            //Now, create an object to send clients
            LightspeedRequest packet = new LightspeedRequest
            {
                auth = metadata.auth,
                endpoint = metadata.endpoint,
                method = metadata.method,
                token = metadata.requestToken,
                client = this,
                body = bodyBytes,
                query = metadata.query
            };

            //Handle
            handler(packet);
        }
    }
}
