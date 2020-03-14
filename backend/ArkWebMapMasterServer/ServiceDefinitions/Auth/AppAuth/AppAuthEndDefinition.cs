using ArkWebMapMasterServer.Services.Auth.AppAuth;
using LibDeltaSystem;
using LibDeltaSystem.WebFramework;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapMasterServer.ServiceDefinitions.Auth.AppAuth
{
    public class AppAuthEndDefinition : DeltaWebServiceDefinition
    {
        public override string GetTemplateUrl()
        {
            return "/auth/steam_auth_return";
        }

        public override DeltaWebService OpenRequest(DeltaConnection conn, HttpContext e)
        {
            return new AppAuthEndRequest(conn, e);
        }
    }
}
