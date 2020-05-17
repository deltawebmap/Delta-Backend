using LibDeltaSystem;
using LibDeltaSystem.WebFramework.ServiceTemplates;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Servers.Admin
{
    public class AdminServerPlayerListRequest : ArkServerAdminDeltaService
    {
        public AdminServerPlayerListRequest(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public override async Task OnAuthenticatedRequest()
        {
            //Get offset and count
            int limit;
            int page;
            if (!TryGetIntFromQuery("limit", out limit))
                limit = 30;
            if (!TryGetIntFromQuery("page", out page))
                page = 0;
            if(limit < 0 || page < 0 || limit > 300)
            {
                await WriteString("Params not within acceptable range.", "text/plain", 400);
                return;
            }

            //Fetch players
            var players = await server.GetPlayerProfiles(conn, page * limit, limit);

            //Create output
            var output = new ResponseData
            {
                players = new List<ResponsePlayer>()
            };

            //Convert
            foreach(var p in players)
            {
                //Try to see if this has a Delta account
                var deltaAccount = conn.GetUserBySteamIdAsync(p.steam_id);

                //Add user
                output.players.Add(new ResponsePlayer
                {
                    ark_id = p.ark_id.ToString(),
                    delta_account = deltaAccount != null,
                    icon = p.icon,
                    last_seen = p.last_seen,
                    name = p.name,
                    steam_id = p.steam_id,
                    tribe_id = p.tribe_id,
                    ark_name = p.ig_name
                });
            }

            //Write
            await WriteJSON(output);
        }

        class ResponseData
        {
            public List<ResponsePlayer> players;
        }

        class ResponsePlayer
        {
            public string name;
            public string ark_name;
            public string icon;
            public int tribe_id;
            public DateTime last_seen;
            public string steam_id;
            public bool delta_account;
            public string ark_id;
        }
    }
}
