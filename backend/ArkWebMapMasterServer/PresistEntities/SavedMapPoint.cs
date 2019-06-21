using ArkWebMapGatewayClient.Messages;
using ArkWebMapGatewayClient.Messages.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapMasterServer.PresistEntities
{
    public class SavedMapPoint
    {
        public int _id { get; set; }

        public string server_id { get; set; }
        public int tribe_id { get; set; }
        public int map_id { get; set; }

        public List<ArkTribeMapDrawingPoint> points { get; set; }
    }
}
