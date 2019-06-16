using ArkWebMapGatewayClient.Messages;
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

namespace ArkWebMapGatewayClient
{
    public class AWMGatewayClient
    {
        public const string REMOTE_CONFIG_ENDPOINT = "https://ark.romanport.com/gateway_config.json";
        public const int CLIENT_LIB_VERSION_MAJOR = 1;
        public const int CLIENT_LIB_VERSION_MINOR = 0;

        public GatewayClientType connectionType;

        public string client_name;
        public string client_name_extra;
        public int client_version_major;
        public int client_version_minor;
        public bool logging_enabled;
        public string token;
        public GatewayMessageHandler handler;

        private GatewayConfigFile config;

        private ClientWebSocket sock;
        private Thread rxThread;
        private Thread txThread;

        private Queue<GatewayMessageBase> txQueue;
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
        public static AWMGatewayClient CreateClient(GatewayClientType type, string client_name, string client_name_extra, int client_version_major, int client_version_minor, bool logging_enabled, GatewayMessageHandler handler, string token)
        {
            //Create object and add args
            AWMGatewayClient client = new AWMGatewayClient
            {
                connectionType = type,
                client_name = client_name,
                client_name_extra = client_name_extra,
                client_version_major = client_version_major,
                client_version_minor = client_version_minor,
                logging_enabled = logging_enabled,
                handler = handler,
                txQueue = new Queue<GatewayMessageBase>(),
                token = token
            };

            //Make a request to the client config endpoint to download the remote configuration file
            string configText;
            try
            {
                using (WebClient wc = new WebClient())
                    configText = wc.DownloadString(REMOTE_CONFIG_ENDPOINT + client.CreateUrlParams());
            } catch (Exception ex)
            {
                throw new Exception("Failed to download remote configuration file.");
            }
            GatewayConfigFile config = JsonConvert.DeserializeObject<GatewayConfigFile>(configText);
            client.config = config;

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

        public void LogMsg(string topic, string msg)
        {
            if (logging_enabled)
                Console.WriteLine($"[AWM GATEWAY Client / {topic}] {msg}");
        }

        /// <summary>
        /// Creates URL params for HTTP requests with the client info
        /// </summary>
        /// <returns></returns>
        public string CreateUrlParams()
        {
            return $"?clientName={HttpUtility.UrlEncode(client_name)}&clientNameExtra={HttpUtility.UrlEncode(client_name_extra)}&clientVersionMajor={client_version_major}&clientVersionMinor={client_version_minor}&clientLibVersionMajor={CLIENT_LIB_VERSION_MAJOR}&clientLibVersionMinor={CLIENT_LIB_VERSION_MINOR}&auth_token={token}";
        }

        /// <summary>
        /// Converts the type we got into the endpoint name for the GATEWAY. REQUIRES the config file
        /// </summary>
        /// <returns></returns>
        public string GetEndpointPathname()
        {
            if (config == null)
                throw new Exception("Config file is null.");
            switch(connectionType)
            {
                case GatewayClientType.Frontend: return config.gateway_endpoints.frontend;
                case GatewayClientType.MasterServer: return config.gateway_endpoints.master;
                case GatewayClientType.SubServer: return config.gateway_endpoints.subserver;
            }
            throw new Exception("Unexpected GatewayClientType.");
        }

        /// <summary>
        /// Tries to make a connection to the WebSocket. If it fails, it starts the retry timer.
        /// </summary>
        /// <returns></returns>
        private async Task TryMakeConnection()
        {
            //Try and make a connection.
            LogMsg("TryMakeConnection", "Attempting connection to GATEWAY.");
            LogMsg("TryMakeConnection", "GATEWAY connection params: "+CreateGatewayURI().ToString());
            try
            {
                await CreateConnection();
                LogMsg("TryMakeConnection", "Successfully created connection to GATEWAY.");

                //Ok. Stop retry timer.
                if (retryTimer != null)
                    retryTimer.Stop();
                retryTimer = null;
            } catch (Exception ex)
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
            Uri uri = new Uri($"{config.gateway_proto}://{config.gateway_host}/v{CLIENT_LIB_VERSION_MAJOR}/{GetEndpointPathname()}{CreateUrlParams()}");
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
                    else if (ar.MessageType == WebSocketMessageType.Binary)
                    {
                        //Binary messages are not supported. Drop.
                        LogMsg("RxBgThread", "Binary messagesa are not supported. Are you sure we're talking to the right server?");
                    }
                    else if (ar.MessageType == WebSocketMessageType.Text)
                    {
                        //Expected this. Handle.
                        string msg = Encoding.UTF8.GetString(buf, 0, ar.Count);
                        OnMsgRx(msg);
                    }
                }
                catch (Exception ex)
                {
                    LogMsg("RxBgThread", "Unexpected fatal error, dropping: " + ex.Message);
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
                if (txQueue.TryDequeue(out GatewayMessageBase msg))
                {
                    try
                    {
                        //Serialize msg
                        string msgPayload = JsonConvert.SerializeObject(msg);
                        byte[] msgSer = Encoding.UTF8.GetBytes(msgPayload);
                        LogMsg("TxBgThread", "Sending message with opcode " + msg.opcode.ToString() + " (" + (int)msg.opcode + ") and size " + msgSer.Length + ".");
                        LogMsg("TxBgThread", "Message payload: " + msgPayload);
                        sock.SendAsync(new ArraySegment<byte>(msgSer), WebSocketMessageType.Text, true, CancellationToken.None);
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

        private void OnMsgRx(string msg)
        {
            //First, deserialize and get the opcode.
            GatewayMessageBase b = JsonConvert.DeserializeObject<GatewayMessageBase>(msg);

            //Log
            LogMsg("OnMsgRx", "Got incoming message with opcode " + b.opcode.ToString() + " (" + (int)b.opcode + ")");
            LogMsg("OnMsgRx", "Message payload: " + msg);

            //Handle
            handler.HandleMsg(b.opcode, msg, this);
        }
    }
}
