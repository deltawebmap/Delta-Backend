using ArkWebMapMasterServer.Services.Servers.Admin;
using LibDeltaSystem;
using LibDeltaSystem.WebFramework;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapMasterServer.ServiceDefinitions.Servers.Admin
{
    public class AdminServerStatsDefinition : DeltaWebServiceDefinition
    {
        public override string GetTemplateUrl()
        {
            return "/servers/{SERVER}/admin/stats";
        }

        public override DeltaWebService OpenRequest(DeltaConnection conn, HttpContext e)
        {
            return new AdminServerStats(conn, e);
        }
    }
}
