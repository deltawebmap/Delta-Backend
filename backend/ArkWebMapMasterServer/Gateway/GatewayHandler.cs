using ArkWebMapGatewayClient;
using ArkWebMapGatewayClient.Messages;
using ArkWebMapMasterServer.Tools;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapMasterServer.Gateway
{
    public class GatewayHandler : GatewayMessageHandler
    {
        public override void Msg_TribeMapBackendOutput(MessageMapDrawingMaster data, object context)
        {
            DrawableMapTool.AddMapPoints(data.server_id, data.tribe_id, data.map_id, data.points);
        }

        public override void Msg_MessageServerStateChange(MessageServerStateChange data, object context)
        {
            if (data.isUp && !Program.onlineServers.Contains(data.serverId))
                Program.onlineServers.Add(data.serverId);
            else if (!data.isUp && Program.onlineServers.Contains(data.serverId))
                Program.onlineServers.Remove(data.serverId);
        }

        public override void Msg_SubserverOfflineDataUpdated(MessageSubserverOfflineDataUpdated data, object context)
        {
            
        }
    }
}
