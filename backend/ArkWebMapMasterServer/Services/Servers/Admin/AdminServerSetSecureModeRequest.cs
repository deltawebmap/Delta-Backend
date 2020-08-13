using LibDeltaSystem;
using LibDeltaSystem.WebFramework.ServiceTemplates;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Servers.Admin
{
    public class AdminServerSetSecureModeRequest : ArkServerAdminDeltaService
    {
        public AdminServerSetSecureModeRequest(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public override async Task OnAuthenticatedRequest()
        {
            //Read
            RequestData request = await ReadPOSTContentChecked<RequestData>();
            if (request == null)
                return;

            //Run if changed
            if(request.secure != server.secure_mode)
            {
                await server.ChangeSecureMode(conn, request.secure);
            }

            await WriteJSON(new ResponseData
            {
                ok = true
            });
        }

        class RequestData
        {
            public bool secure;
        }

        class ResponseData
        {
            public bool ok;
        }
    }
}
