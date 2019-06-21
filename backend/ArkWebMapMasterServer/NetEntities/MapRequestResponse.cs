using ArkWebMapGatewayClient.Messages.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapMasterServer.NetEntities
{
    public class MapRequestResponse
    {
        public string name;
        public int id;
        public List<ArkTribeMapDrawingPoint> points;
    }
}
