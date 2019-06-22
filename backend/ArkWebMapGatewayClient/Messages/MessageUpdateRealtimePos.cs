using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapGatewayClient.Messages
{
    public class MessageUpdateRealtimePos : GatewayMessageBase
    {
        public string id; //ID of the object being moved

        public UpdateTypeOpcode updateOpcode; //Type moved

        public float mx; //map x
        public float my; //map y

        public float ox; //original x
        public float oy; //original y
        public float oz; //original z

        public enum UpdateTypeOpcode
        {
            Player = 0,
            Dino = 1
        }
    }
}
