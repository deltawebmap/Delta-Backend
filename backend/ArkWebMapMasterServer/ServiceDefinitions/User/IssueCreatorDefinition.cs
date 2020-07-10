using ArkWebMapMasterServer.Services.Users;
using LibDeltaSystem;
using LibDeltaSystem.WebFramework;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapMasterServer.ServiceDefinitions.User
{
    public class IssueCreatorDefinition : DeltaWebServiceDefinition
    {
        public override string GetTemplateUrl()
        {
            return "/users/@me/report_issue";
        }

        public override DeltaWebService OpenRequest(DeltaConnection conn, HttpContext e)
        {
            return new IssueCreator(conn, e);
        }
    }
}
