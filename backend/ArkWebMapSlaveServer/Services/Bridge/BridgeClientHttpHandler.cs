using ArkBridgeSharedEntities.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapSlaveServer.Services.Bridge
{
    public class BridgeClientHttpHandler
    {
        public static Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, string path)
        {
            if(path == "ping")
            {
                //Return the same content back.
                return e.Request.Body.CopyToAsync(e.Response.Body);
            }

            //Not found
            throw new StandardError("Not Found", StandardErrorCode.NotFound);
        }
    }
}
