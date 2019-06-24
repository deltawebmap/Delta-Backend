using ArkBridgeSharedEntities.Entities;
using ArkWebMapLightspeedClient;
using ArkWebMapLightspeedClient.Entities;
using ArkWebMapSlaveServer.NetEntities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapSlaveServer
{
    public class HttpHandler
    {
        public static async Task OnHttpRequest(LightspeedRequest e)
        {
            try
            {
                //Authenticate the user
                MasterServerArkUser user = e.auth;

                //Handle
                await ArkHttpServer.ArkWebServer.OnHttpRequest(e, user);
                return;

                //Unknown
                throw new StandardError("Not Found", StandardErrorCode.NotFound);
            }
            catch (StandardError ex)
            {
                //Write error
                ArkWebMapServer.Log($"Request hit an error: {ex.screen_error} ({ex.error_code_string})", ConsoleColor.Yellow);
                await e.DoRespondJson(ex, 500);
                return;
            }
            catch (Exception ex)
            {
                //Write error
                ArkWebMapServer.Log($"Request hit an error: {ex.Message} at {ex.StackTrace.Replace("\n", "")}", ConsoleColor.Yellow);
                await e.DoRespondJson(new StandardError(ex.Message + ex.StackTrace, StandardErrorCode.UncaughtException, ex), 500);
                return;
            }
        }
    }
}
