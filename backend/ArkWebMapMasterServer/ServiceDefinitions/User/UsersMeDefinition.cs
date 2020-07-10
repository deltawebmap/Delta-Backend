using ArkWebMapMasterServer.Services.Users;
using LibDeltaSystem;
using LibDeltaSystem.WebFramework;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapMasterServer.ServiceDefinitions.User
{
    public class UsersMeDefinition : DeltaWebServiceDefinition
    {
        public override string GetTemplateUrl()
        {
            return "/users/@me";
        }

        public override DeltaWebService OpenRequest(DeltaConnection conn, HttpContext e)
        {
            return new UsersMeRequest(conn, e);
        }
    }
}
