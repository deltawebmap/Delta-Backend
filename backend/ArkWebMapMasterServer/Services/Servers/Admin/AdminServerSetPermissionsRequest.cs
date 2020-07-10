using LibDeltaSystem;
using LibDeltaSystem.Db.System;
using LibDeltaSystem.WebFramework.ServiceTemplates;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Servers.Admin
{
    public class AdminServerSetPermissionsRequest : ArkServerAdminDeltaService
    {
        public AdminServerSetPermissionsRequest(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public override async Task OnAuthenticatedRequest()
        {
            //Make sure we're admin
            if (!server.IsUserOwner(user))
            {
                await WriteString("Only the server owner can change this setting.", "text/plain", 400);
                return;
            }

            //Read
            RequestData request = await ReadPOSTContentChecked<RequestData>();
            if (request == null)
                return;

            //Update
            await server.ChangePermissionFlags(conn, (int)request.flags);

            //Update template if needed
            if(request.template != null)
            {
                await server.ExplicitUpdateAsync(conn, MongoDB.Driver.Builders<DbServer>.Update.Set("permissions_template", request.template));
                server.permissions_template = request.template;
                await server.NotifyPublicDetailsChanged(conn);
            }

            await WriteJSON(new ResponseData
            {
                ok = true
            });
        }

        class RequestData
        {
            public uint flags;
            public string template;
        }

        class ResponseData
        {
            public bool ok;
        }
    }
}
