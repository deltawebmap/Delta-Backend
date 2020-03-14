using ArkWebMapMasterServer.Services.Misc;
using LibDeltaSystem;
using LibDeltaSystem.WebFramework;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapMasterServer.ServiceDefinitions.Misc
{
    public class MapListDefinition : DeltaWebServiceDefinition
    {
        public override string GetTemplateUrl()
        {
            return "/maps.json";
        }

        public override DeltaWebService OpenRequest(DeltaConnection conn, HttpContext e)
        {
            return new MapListRequest(conn, e);
        }
    }
}
