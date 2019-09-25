using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapGatewayClient
{
    public class AWMGatewayClient : GenericGatewayClient
    {
        /// <summary>
        /// Creates a new connection
        /// </summary>
        /// <param name="type"></param>
        /// <param name="client_name"></param>
        /// <param name="client_name_extra"></param>
        /// <param name="client_version_major"></param>
        /// <param name="client_version_minor"></param>
        /// <param name="logging_enabled"></param>
        /// <param name="handler"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static AWMGatewayClient CreateClient(GatewayClientType type, string client_name, string client_name_extra, int client_version_major, int client_version_minor, bool logging_enabled, GatewayMessageHandler handler, string token)
        {
            AWMGatewayClient client = new AWMGatewayClient();
            client.InternalSetupClient(type, client_name, client_name_extra, client_version_major, client_version_minor, logging_enabled, handler, token);
            return client;
        }

        /// <summary>
        /// Queues a message
        /// </summary>
        public void SendMessage(GatewayMessageBase msg)
        {
            SendMessage(JsonConvert.SerializeObject(msg));
        }

        /// <summary>
        /// Queues a message
        /// </summary>
        public void SendMessage(string msg)
        {
            QueueMessage(Encoding.UTF8.GetBytes(msg));
        }
    }
}
