using ArkWebMapMasterServer.Services.Users;
using LibDeltaSystem;
using LibDeltaSystem.WebFramework;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapMasterServer.ServiceDefinitions.User
{
    public class TokenDevalidateDefinition : DeltaWebServiceDefinition
    {
        public override string GetTemplateUrl()
        {
            return "/users/@me/tokens/{TOKEN_TYPE}/devalidate";
        }

        public override DeltaWebService OpenRequest(DeltaConnection conn, HttpContext e)
        {
            return new TokenDevalidateService(conn, e);
        }
    }
}
