using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapGatewayClient.Messages
{
    public class MessageOnMirrorDinoUpdate : GatewayMessageBase
    {
        public string dino_id;
        public string server_id;
        public DateTime time;
        public Dictionary<string, object> updates;
    }
}
