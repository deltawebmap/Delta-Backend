using ArkBridgeSharedEntities.Entities.Master;
using ArkWebMapMasterServer.NetEntities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapGateway.Entities
{
    public class GatewayFrontendMsgMeta
    {
        public string user_id;
        public string server_id;
        public UsersMeReply_Server server;
        public int tribe_id;
    }
}
