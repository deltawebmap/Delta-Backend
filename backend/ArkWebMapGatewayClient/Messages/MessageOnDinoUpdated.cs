using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapGatewayClient.Messages
{
    public class MessageOnDinoUpdated : GatewayMessageBase
    {
        public DateTime time;
        public string server_id;
        public List<MessageOnDinoUpdated_Dino> dinos;
    }

    public class MessageOnDinoUpdated_Dino
    {
        public string name;
        public string classname;
        public string icon;
        public int level;
        public string status;
        public float x;
        public float y;
        public float z;
        public string id;
    }
}
