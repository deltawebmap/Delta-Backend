using ArkWebMapMasterServer.Services.Servers.Admin;
using LibDeltaSystem;
using LibDeltaSystem.WebFramework;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapMasterServer.ServiceDefinitions.Servers.Admin
{
    public class AdminServerUpdateDefinition : DeltaWebServiceDefinition
    {
        public AdminServerUpdateDefinition()
        {
        }

        public override string GetTemplateUrl()
        {
            return "/servers/{SERVER}/admin/update";
        }

        public override DeltaWebService OpenRequest(DeltaConnection conn, HttpContext e)
        {
            return new AdminServerUpdateRequest(conn, e);
        }
    }
}
