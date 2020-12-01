using LibDeltaSystem;
using LibDeltaSystem.Db.System;
using LibDeltaSystem.Entities.CommonNet;
using LibDeltaSystem.WebFramework;
using LibDeltaSystem.WebFramework.ServiceTemplates;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Servers.Admin
{
    public class AdminServerUpdateRequest : ArkServerAdminDeltaService
    {
        public AdminServerUpdateRequest(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public override async Task OnAuthenticatedRequest()
        {
            //Get builder
            var builder = server.GetUpdateBuilder(conn);

            //Get data
            NetGuildSettings request = await DecodePOSTBody<NetGuildSettings>();

            //Apply
            if (request.name != null)
                builder.UpdateServerNameValidated(request.name);
            if (request.permission_flags != null)
                builder.UpdatePermissionFlags(request.permission_flags.Value);
            if (request.permissions_template != null)
                builder.UpdatePermissionTemplate(request.permissions_template);
            if (request.is_locked != null)
                builder.UpdateFlag(DbServer.FLAG_INDEX_LOCKED, request.is_locked.Value);
            if (request.is_secure != null)
                builder.UpdateFlag(DbServer.FLAG_INDEX_SECURE, request.is_secure.Value);

            //If we've updated enough settings to change if the server needs configuration, do that
            if(request.name != null && request.permission_flags != null && request.permissions_template != null)
                builder.UpdateFlag(DbServer.FLAG_INDEX_SETUP, false);

            //Apply
            await builder.Apply();

            //Write
            await WriteJSON(NetGuild.GetGuild(server));
        }

    }
}
