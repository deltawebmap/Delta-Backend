using ArkWebMapMasterServer.ServiceTemplates;
using LibDeltaSystem;
using LibDeltaSystem.Db.System;
using LibDeltaSystem.WebFramework.ServiceTemplates;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Servers.Admin
{
    public class AdminServerStats : ArkServerAdminDeltaService
    {
        public AdminServerStats(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public const int VERSION_CURRENT = 15;
        public const int VERSION_VERY_OUTDATED = 14;

        public override async Task OnAuthenticatedRequest()
        {
            //Fetch pings
            var rawPings = await (await conn.system_server_pings.FindAsync(Builders<DbServerPing>.Filter.Eq("server_id", server._id), new FindOptions<DbServerPing, DbServerPing>
            {
                Limit = 960, //8 hours
                Sort = Builders<DbServerPing>.Sort.Descending("time")
            })).ToListAsync();

            //Convert pings
            List<ResponseData_Ping> pings = new List<ResponseData_Ping>();
            foreach (var p in rawPings)
                pings.Add(new ResponseData_Ping
                {
                    time = p.time,
                    player_count = p.player_count,
                    tick_avg = p.avg_tick_seconds,
                    tick_max = p.max_tick_seconds,
                    tick_min = p.min_tick_seconds
                });

            //Get status string
            string status = "OFFLINE";
            if((DateTime.UtcNow - server.last_pinged_time).TotalSeconds < 60)
            {
                //Online
                status = "ONLINE";
                if (server.last_client_version < VERSION_CURRENT)
                    status = "OUTDATED_SERVER";
                if (server.last_client_version <= VERSION_VERY_OUTDATED)
                    status = "VERY_OUTDATED_SERVER";
            }

            //Create response
            await WriteJSON(new ResponseData
            {
                status = status,
                last_started = server.last_connected_time,
                last_ping = server.last_pinged_time,
                last_version = server.last_client_version,
                pings = pings
            });
        }

        class ResponseData
        {
            public string status; //Can be one of "ONLINE", "OFFLINE", "OUTDATED_SERVER", "VERY_OUTDATED_SERVER"
            public DateTime last_started;
            public DateTime last_ping;
            public int last_version;
            public List<ResponseData_Ping> pings;
        }

        class ResponseData_Ping
        {
            public DateTime time;
            public int player_count;
            public float tick_max;
            public float tick_avg;
            public float tick_min;
        }
    }
}
