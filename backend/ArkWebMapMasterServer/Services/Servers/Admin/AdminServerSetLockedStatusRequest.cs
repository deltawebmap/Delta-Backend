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

            //Clear setup flag
            server.flags &= ~(1 << 1);
            
            //Change lock flag
            if(payload.locked)
                server.flags |= 1 << 0;
            else
                server.flags &= ~(1 << 0);

            //Update
            await server.ExplicitUpdateAsync(conn, Builders<DbServer>.Update.Set("flags", server.flags));

            //Notify via RPC
            await server.NotifyPublicDetailsChanged(conn);

            //Return server
            await WriteJSON(await NetGuildUser.GetNetGuild(conn, server, user));
        }

        class RequestData
        {
            public bool locked;
        }
    }
}
