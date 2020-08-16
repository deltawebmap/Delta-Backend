using LibDeltaSystem;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.ServiceTemplates
{
    public abstract class MasterArkRpcAdminService : MasterArkRpcService
    {
        public MasterArkRpcAdminService(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public override async Task OnRequest()
        {
            //Make sure we're an admin
            if (server.CheckIsUserAdmin(user))
            {
                await base.OnRequest();
            }
            else
            {
                await WriteString("Only server admins can access this endpoint.", "text/plain", 401);
            }
        }
    }
}
