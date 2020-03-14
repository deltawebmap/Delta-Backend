using ArkWebMapMasterServer.Services.Auth.OAuth;
using LibDeltaSystem;
using LibDeltaSystem.WebFramework;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapMasterServer.ServiceDefinitions.Auth.OAuth
{
    public class OAuthAuthorizeDefinition : DeltaWebServiceDefinition
    {
        public override string GetTemplateUrl()
        {
            return "/auth/oauth/authorize";
        }

        public override DeltaWebService OpenRequest(DeltaConnection conn, HttpContext e)
        {
            return new OAuthAuthorize(conn, e);
        }
    }
}
