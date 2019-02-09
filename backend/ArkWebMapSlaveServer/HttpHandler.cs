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
                /*string calculated_hmac = ArkBridgeSharedEntities.HMACGen.GenerateHMAC(e.Request.Path + e.Request.QueryString.ToString(), e.Request.Method, Convert.FromBase64String(e.Request.Headers["X-Ark-Salt"]), ArkWebMapServer.creds);
                if(calculated_hmac != e.Request.Headers["X-Ark-Integrity"])
                {
                    ArkWebMapServer.Log($"Warning: IP {e.Request.Headers["X-Ark-Source-IP"]} sent invalid integrity data. Dropping request.", ConsoleColor.Yellow);
                    throw new StandardError("Integrity check failed.", StandardErrorCode.BridgeIntegrityCheckFailed);
                }*/

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
                return ArkWebMapServer.QuickWriteJsonToDoc(e, ex, 500);
            }
            catch (Exception ex)
            {
                //Write error
                return ArkWebMapServer.QuickWriteJsonToDoc(e, new StandardError(ex.Message + ex.StackTrace, StandardErrorCode.UncaughtException, ex), 500);
            }
        }
    }
}
