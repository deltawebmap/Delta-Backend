using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapGatewayClient.Sender
{
    public class SenderMsg
    {
        public SenderMsgType type;
        public SenderMsgQuery query;
        public JObject payload; //Only in use when type == Relay
    }

    public class SenderMsgQuery
    {
        public string client_type; //Type of client
        public string index_name; //Index to search for, or "ID" for ID
        public string index_value; //Index value
    }

    public enum SenderMsgType
    {
        Relay, //Relay the message
        IdUpdate //Update indexes of an object by ID
    }
}
