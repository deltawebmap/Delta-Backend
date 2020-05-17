using ArkWebMapMasterServer.Services.Servers;
using LibDeltaSystem;
using LibDeltaSystem.WebFramework;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapMasterServer.ServiceDefinitions.Servers
{
    public class LeaveServerDefinition : DeltaWebServiceDefinition
    {
        public override string GetTemplateUrl()
        {
            return "/servers/{SERVER}/leave";
        }

        public override DeltaWebService OpenRequest(DeltaConnection conn, HttpContext e)
        {
            return new LeaveServerRequest(conn, e);
        }
    }
}
