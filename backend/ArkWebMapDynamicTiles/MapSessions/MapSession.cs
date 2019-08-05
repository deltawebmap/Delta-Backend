using ArkBridgeSharedEntities.Entities.Master;
using ArkWebMapDynamicTiles.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapDynamicTiles.MapSessions
{
    public abstract class MapSession
    {
        public string server_id;
        public string user_id;
        public int tribe_id;

        public DateTime last_heartbeat;

        public abstract Task OnCreate(Microsoft.AspNetCore.Http.HttpContext e, UsersMeReply_Server server, ContentMetadata commit);
        public abstract Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, float x, float y, float z);
        public abstract int GetMinDataVersion();
    }
}
