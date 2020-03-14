using ArkWebMapMasterServer.Services.Misc;
using LibDeltaSystem;
using LibDeltaSystem.WebFramework;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapMasterServer.ServiceDefinitions.Misc
{
    public class PreregisterUserDefinition : DeltaWebServiceDefinition
    {
        public override string GetTemplateUrl()
        {
            return "/preregister";
        }

        public override DeltaWebService OpenRequest(DeltaConnection conn, HttpContext e)
        {
            return new PreregisterUserRequest(conn, e);
        }
    }
}
