using ArkWebMapMasterServer.Services.Servers;
using LibDeltaSystem;
using LibDeltaSystem.WebFramework;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapMasterServer.ServiceDefinitions.Servers
{
    public class ServerTribesDefinition : DeltaWebServiceDefinition
    {
        public ServerTribesDefinition()
        {
        }

        public override string GetTemplateUrl()
        {
            return "/servers/{SERVER}/tribes";
        }

        public override DeltaWebService OpenRequest(DeltaConnection conn, HttpContext e)
        {
            return new ServerTribesRequest(conn, e);
        }
    }
}
