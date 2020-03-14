using ArkWebMapMasterServer.Services.Auth.AppAuth;
using LibDeltaSystem;
using LibDeltaSystem.WebFramework;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapMasterServer.ServiceDefinitions.Auth.AppAuth
{
    public class AppAuthTokenDefinition : DeltaWebServiceDefinition
    {
        public override string GetTemplateUrl()
        {
            return "/auth/token";
        }

        public override DeltaWebService OpenRequest(DeltaConnection conn, HttpContext e)
        {
            return new AppAuthTokenRequest(conn, e);
        }
    }
}
