using ArkWebMapMasterServer.Services.Users;
using LibDeltaSystem;
using LibDeltaSystem.WebFramework;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapMasterServer.ServiceDefinitions.User
{
    public class PutUserSettingsDefinition : DeltaWebServiceDefinition
    {
        public override string GetTemplateUrl()
        {
            return "/users/@me/user_settings";
        }

        public override DeltaWebService OpenRequest(DeltaConnection conn, HttpContext e)
        {
            return new PutUserSettingsRequest(conn, e);
        }
    }
}
