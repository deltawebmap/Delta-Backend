using ArkWebMapMasterServer.Services.Misc;
using LibDeltaSystem;
using LibDeltaSystem.WebFramework;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapMasterServer.ServiceDefinitions.Misc
{
    public class FetchPlatformProfileDefinition : DeltaWebServiceDefinition
    {
        public override string GetTemplateUrl()
        {
            return "/fetch_platform_profiles";
        }

        public override DeltaWebService OpenRequest(DeltaConnection conn, HttpContext e)
        {
            return new FetchPlatformProfileService(conn, e);
        }
    }
}
