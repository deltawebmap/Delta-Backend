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
    public class AdminServerCompleteInitialSetupRequest : ArkServerAdminDeltaService
    {
        public AdminServerCompleteInitialSetupRequest(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public override async Task OnAuthenticatedRequest()
        {
            //Make sure we need setup
            if (server.CheckFlag(DbServer.FLAG_INDEX_SETUP))
                throw new DeltaWebException("Server setup has already been completed.", 400);
            
            //Get data
            RequestData request = await DecodePOSTBody<RequestData>();

            //Apply
            await server.GetUpdateBuilder(conn)
                .UpdateServerNameValidated(request.server_name)
                .UpdatePermissionTemplate(request.permission_template)
                .UpdatePermissionFlags(request.permission_flags)
                .UpdateFlag(DbServer.FLAG_INDEX_SETUP, false)
                .UpdateFlag(DbServer.FLAG_INDEX_LOCKED, request.locked)
                .Apply();

            //Write
            await WriteJSON(NetGuild.GetGuild(server));
        }

        class RequestData
        {
            public string server_name;
            public int permission_flags;
            public string permission_template;
            public bool locked;
        }
    }
}
