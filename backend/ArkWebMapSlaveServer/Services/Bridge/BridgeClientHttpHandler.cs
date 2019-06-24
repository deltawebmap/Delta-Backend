using ArkBridgeSharedEntities.Entities;
using ArkWebMapLightspeedClient;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapSlaveServer.Services.Bridge
{
    public class BridgeClientHttpHandler
    {
        public static async Task OnHttpRequest(LightspeedRequest e, string path)
        {
            //Not found
            throw new StandardError("Not Found", StandardErrorCode.NotFound);
        }
    }
}
