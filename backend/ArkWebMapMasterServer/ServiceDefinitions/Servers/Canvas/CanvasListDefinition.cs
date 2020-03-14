using ArkWebMapMasterServer.Services.Servers.Canvas;
using LibDeltaSystem;
using LibDeltaSystem.WebFramework;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapMasterServer.ServiceDefinitions.Servers.Canvas
{
    public class CanvasListDefinition : DeltaWebServiceDefinition
    {
        public override string GetTemplateUrl()
        {
            return "/servers/{SERVER}/canvas";
        }

        public override DeltaWebService OpenRequest(DeltaConnection conn, HttpContext e)
        {
            return new CanvasListRequest(conn, e);
        }
    }
}
