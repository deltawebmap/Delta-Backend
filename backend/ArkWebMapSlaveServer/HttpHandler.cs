using ArkBridgeSharedEntities.Entities;
using ArkWebMapSlaveServer.NetEntities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapSlaveServer
{
    public class HttpHandler
    {
        public static Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e)
        {
            try
            {
                //Verify the integrity of the connection.
                if(!e.Request.Headers.ContainsKey("X-Ark-Salt") || !e.Request.Headers.ContainsKey("X-Ark-Integrity"))
                {
                    //Missing data
                    ArkWebMapServer.Log($"Warning: Client did not send required integrity data. Dropping request.", ConsoleColor.Yellow);
                    throw new StandardError("Integrity check failed. Missing 'X-Ark-Salt' or 'X-Ark-Integrity'.", StandardErrorCode.BridgeIntegrityCheckFailed);
                }
                string calculated_hmac = ArkBridgeSharedEntities.HMACGen.GenerateHMAC(Convert.FromBase64String(e.Request.Headers["X-Ark-Salt"]), ArkWebMapServer.creds);
                if (calculated_hmac != e.Request.Headers["X-Ark-Integrity"])
                {
                    ArkWebMapServer.Log($"Warning: IP {e.Request.Headers["X-Ark-Source-IP"]} sent invalid integrity data to {e.Request.Path}. Dropping request.", ConsoleColor.Red);
                    throw new StandardError("Integrity check failed.", StandardErrorCode.BridgeIntegrityCheckFailed);
                }

                //Authenticate the user
                ArkHttpServer.Entities.MasterServerArkUser user = JsonConvert.DeserializeObject<ArkHttpServer.Entities.MasterServerArkUser>(e.Request.Headers["X-Ark-User-Auth"]);
                
                //Read the first part of the path.
                string path = e.Request.Path.ToString();
                if (path.StartsWith("/bridge/"))
                {
                    //Pass onto this part
                    return Services.Bridge.BridgeClientHttpHandler.OnHttpRequest(e, path.Substring("/bridge/".Length));
                }
                if (path.StartsWith("/api/"))
                {
                    //This is a proxy-ed request. Redirect
                    return ArkHttpServer.ArkWebServer.OnHttpRequest(e, user);
                }

                //Unknown
                throw new StandardError("Not Found", StandardErrorCode.NotFound);
            }
            catch (StandardError ex)
            {
                //Write error
                ArkWebMapServer.Log($"Request hit an error: {ex.screen_error} ({ex.error_code_string})", ConsoleColor.Yellow);
                return ArkWebMapServer.QuickWriteJsonToDoc(e, ex, 500);
            }
            catch (Exception ex)
            {
                //Write error
                ArkWebMapServer.Log($"Request hit an error: {ex.Message} at {ex.StackTrace.Replace("\n", "")}", ConsoleColor.Yellow);
                return ArkWebMapServer.QuickWriteJsonToDoc(e, new StandardError(ex.Message + ex.StackTrace, StandardErrorCode.UncaughtException, ex), 500);
            }
        }
    }
}
