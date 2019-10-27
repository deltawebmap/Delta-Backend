using ArkWebMapDynamicTiles.Entities;
using LibDeltaSystem.Db.System;
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

        public bool is_premium;
        public int resolution;

        public DateTime last_heartbeat;

        public abstract Task OnCreate(Microsoft.AspNetCore.Http.HttpContext e, DbServer server, int tribeId);
        public abstract Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, float x, float y, float z);
        public abstract int GetMinDataVersion();
        public abstract int GetMaxZoom();
    }
}
