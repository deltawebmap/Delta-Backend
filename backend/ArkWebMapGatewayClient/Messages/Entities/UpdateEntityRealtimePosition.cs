using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapGatewayClient.Messages.Entities
{
    public class UpdateEntityRealtimePosition
    {
        public string id; //ID of the object being moved

        public UpdateTypeOpcode t; //Type moved

        public float mx; //map x
        public float my; //map y

        public enum UpdateTypeOpcode
        {
            Player = 1,
            Dino = 0
        }
    }
}
