using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapGatewayClient.Messages
{
    public class MessageSubserverOfflineDataUpdated : GatewayMessageBase
    {
        public string server_id;
        public int data_version;
    }
}
