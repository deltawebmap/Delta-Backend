using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapGatewayClient.Messages
{
    public class MessageOnDrawableMapChange : GatewayMessageBase
    {
        public MessageOnDrawableMapChangeOpcode mapOpcode;
        public int id;
        public string name;

        public enum MessageOnDrawableMapChangeOpcode
        {
            Delete,
            Create,
            Rename,
            Clear
        }
    }
}
