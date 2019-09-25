using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using ArkWebMapGatewayClient;
using ArkWebMapGatewayClient.Messages;
using ArkWebMapGatewayClient.Sender;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace ArkWebMapGateway
{
    public abstract class GatewayConnection : WebsocketConnection
    {
        /// <summary>
        /// Connection type
        /// </summary>
        public string type;

        /// <summary>
        /// Connection ID. Could be multiple with this ID, as it is a user ID
        /// </summary>
        public string id;

        /// <summary>
        /// Indexes to search for when sending messages
        /// </summary>
        public Dictionary<string, List<string>> indexes;

        /// <summary>
        /// Checks if a query matches this
        /// </summary>
        /// <returns></returns>
        public bool CheckQuery(SenderMsgQuery query)
        {
            //Check type
            if (type != query.client_type)
                return false;

            //If the index name is "ID", check the id
            if (query.index_name == "ID" && query.index_value == id)
                return true;

            //Check index
            if (!indexes.ContainsKey(query.index_name))
                return false;
            return indexes[query.index_name].Contains(query.index_value);
        }

        public override Task<bool> OnOpen(HttpContext e)
        {
            return Task.FromResult<bool>(true);
        }

        public override Task<bool> OnMsg(string msg)
        {
            throw new NotImplementedException();
        }

        public override Task<bool> OnClose(WebSocketCloseStatus? status)
        {
            return Task.FromResult<bool>(true);
        }

        public abstract Task RefreshIndexes();

        public void OnSetHandler(GatewayHandler handler)
        {
            //Send set ID
            SendMsg(new MessageSetSessionID
            {
                opcode = GatewayMessageOpcode.SetSessionId,
                sessionId = handler.sessionId
            });
        }
    }
}
