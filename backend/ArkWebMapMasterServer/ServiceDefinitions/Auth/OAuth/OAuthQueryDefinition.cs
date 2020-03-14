using ArkWebMapMasterServer.ServiceDefinitions.Auth.AppAuth;
using ArkWebMapMasterServer.Services.Auth.OAuth;
using LibDeltaSystem;
using LibDeltaSystem.WebFramework;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapMasterServer.ServiceDefinitions.Auth.OAuth
{
    public class OAuthQueryDefinition : DeltaWebServiceDefinition
    {
        public override string GetTemplateUrl()
        {
            return "/auth/oauth/query";
        }

        public override DeltaWebService OpenRequest(DeltaConnection conn, HttpContext e)
        {
            return new OAuthQuery(conn, e);
        }
    }
}
