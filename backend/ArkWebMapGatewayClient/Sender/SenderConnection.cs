using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapGatewayClient.Sender
{
    public class SenderConnection : GenericGatewayClient
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
        public static SenderConnection CreateClient(string client_name, string client_name_extra, int client_version_major, int client_version_minor, bool logging_enabled, string token)
        {
            SenderConnection client = new SenderConnection();
            client.InternalSetupClient(GatewayClientType.Sender, client_name, client_name_extra, client_version_major, client_version_minor, logging_enabled, new GatewayMessageHandler(), token);
            return client;
        }

        /// <summary>
        /// Sends a message to a user in a tribe
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="serverId"></param>
        /// <param name="tribeId"></param>
        public void SendMessageToUserInTribeId(GatewayMessageBase msg, string serverId, int tribeId)
        {
            SendMessageToClient(msg, new SenderMsgQuery
            {
                client_type = "USER",
                index_name = "TRIBES",
                index_value = serverId+"/"+tribeId.ToString()
            });
        }

        /// <summary>
        /// Sends a message to a user in a server
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="id"></param>
        public void SendMessageToUserInServerId(GatewayMessageBase msg, string serverId)
        {
            SendMessageToClient(msg, new SenderMsgQuery
            {
                client_type = "USER",
                index_name = "SERVERS",
                index_value = serverId
            });
        }

        /// <summary>
        /// Sends a message to a user with ID
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="id"></param>
        public void SendMessageToUserWithId(GatewayMessageBase msg, string id)
        {
            SendMessageToClient(msg, new SenderMsgQuery
            {
                client_type = "USER",
                index_name = "ID",
                index_value = id
            });
        }

        /// <summary>
        /// Sends a message to a client with a query
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="query"></param>
        public void SendMessageToClient(GatewayMessageBase payload, SenderMsgQuery query)
        {
            SendMessage(new SenderMsg
            {
                payload = JObject.FromObject(payload),
                query = query,
                type = SenderMsgType.Relay
            });
        }

        /// <summary>
        /// Makes the gateway refresh indexes on a client.
        /// </summary>
        /// <param name="id"></param>
        public void SendRefreshIndex(string type, string id)
        {
            SendMessage(new SenderMsg
            {
                type = SenderMsgType.IdUpdate,
                payload = null,
                query = new SenderMsgQuery
                {
                    client_type = type,
                    index_name = "ID",
                    index_value = id
                }
            });
        }

        /// <summary>
        /// Queues a message
        /// </summary>
        private void SendMessage(SenderMsg msg)
        {
            QueueMessage(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(msg)));
        }
    }
}
