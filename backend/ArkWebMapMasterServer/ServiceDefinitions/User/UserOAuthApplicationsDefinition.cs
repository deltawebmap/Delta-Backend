using ArkWebMapMasterServer.Services.Users;
using LibDeltaSystem;
using LibDeltaSystem.Db.System;
using LibDeltaSystem.WebFramework;
using LibDeltaSystem.WebFramework.ServiceTemplates;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapMasterServer.ServiceDefinitions.User
{
    public class UserOAuthApplicationsDefinition_Root : DeltaWebServiceDefinition
    {
        public override string GetTemplateUrl()
        {
            return "/users/@me/applications";
        }

        public override DeltaWebService OpenRequest(DeltaConnection conn, HttpContext e)
        {
            return new OAuthApplicationsHandler(conn, e);
        }
    }

    public class UserOAuthApplicationsDefinition_Item : DeltaWebServiceDefinition
    {
        public override string GetTemplateUrl()
        {
            return "/users/@me/applications/{" + SelectItemUserAuthDeltaService<DbOauthApp>.SELECT_ITEM_ARG + "}";
        }

        public override DeltaWebService OpenRequest(DeltaConnection conn, HttpContext e)
        {
            return new OAuthApplicationsHandler(conn, e);
        }
    }
}
