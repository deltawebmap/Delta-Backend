using ArkWebMapMasterServer.Services.Servers;
using LibDeltaSystem;
using LibDeltaSystem.WebFramework;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapMasterServer.ServiceDefinitions.Servers
{
    public class ServerManageDefinition : DeltaWebServiceDefinition
    {
        public ServerManageDefinition()
        {
        }

        public override string GetTemplateUrl()
        {
            return "/servers/{SERVER}/manage";
        }

        public override DeltaWebService OpenRequest(DeltaConnection conn, HttpContext e)
        {
            return null; //TODO
            //return new ManageRequest(conn, e);
        }
    }
}
