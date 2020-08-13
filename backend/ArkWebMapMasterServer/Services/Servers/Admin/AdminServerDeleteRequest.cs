using LibDeltaSystem;
using LibDeltaSystem.WebFramework.ServiceTemplates;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Servers.Admin
{
    public class AdminServerDeleteRequest : ArkServerAdminDeltaService
    {
        public AdminServerDeleteRequest(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public override async Task OnAuthenticatedRequest()
        {
            //Make sure this is a POST
            if(GetMethod() != LibDeltaSystem.WebFramework.Entities.DeltaCommonHTTPMethod.POST)
            {
                await WriteString("Only POST requests are allowed here.", "text/plain", 400);
                return;
            }

            //Delete
            await server.DeleteServer(conn);

            //Write OK
            await WriteJSON(new ResponseData
            {
                ok = true
            });
        }

        class ResponseData
        {
            public bool ok;
        }
    }
}
