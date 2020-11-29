using LibDeltaSystem;
using LibDeltaSystem.Db.System;
using LibDeltaSystem.Entities.CommonNet;
using LibDeltaSystem.WebFramework.ServiceTemplates;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Servers.Admin
{
    public class AdminServerSetLockedStatusRequest : ArkServerAdminDeltaService
    {
        public AdminServerSetLockedStatusRequest(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public override async Task OnAuthenticatedRequest()
        {
            //Get
            RequestData payload = await ReadPOSTContentChecked<RequestData>();
            if (payload == null)
                return;

            //Apply
            await server.GetUpdateBuilder(conn)
                .UpdateFlag(DbServer.FLAG_INDEX_LOCKED, payload.locked)
                .Apply();

            //Return server
            await WriteJSON(await NetGuildUser.GetNetGuild(conn, server, user));
        }

        class RequestData
        {
            public bool locked;
        }
    }
}
