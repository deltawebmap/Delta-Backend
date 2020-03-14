using ArkWebMapMasterServer.NetEntities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer
{
    public class HttpHandler
    {
        public static async Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e)
        {
            try
            {
                //Read the first part of the path.
                string path = e.Request.Path.ToString();
                if (path.StartsWith("/users/"))
                    await Services.Users.UsersHttpHandler.OnHttpRequest(e, path.Substring("/users/".Length));
                else if (path.StartsWith("/servers/"))
                    await Services.Servers.ServersHttpHandler.OnHttpRequest(e, path.Substring("/servers/".Length));
                else if (path == "/auth/steam_auth")
                    await Services.Auth.AuthHttpHandler.OnBeginRequest(e);
                else if (path == "/auth/steam_auth_return")
                    await Services.Auth.AuthHttpHandler.OnEndRequest(e);
                else if (path == "/auth/token")
                    await Services.Auth.AuthHttpHandler.OnTokenRequest(e);
                else if (path == "/status")
                    await Services.Misc.ServiceStatus.OnHttpRequest(e);
                else if (path == "/download_token")
                    await Tools.TokenFileDownloadTool.OnDownloadRequest(e);
                else if (path == "/preregister")
                    await Services.Misc.PreregisterUser.OnHttpRequest(e);
                else if (path == "/maps.json")
                    await Services.Misc.MapList.OnHttpRequest(e);
                else
                    throw new StandardError("Not Found", StandardErrorCode.NotFound);
            } catch (StandardError ex)
            {
                //Set status code
                int errorCode = 500;
                if (ex.error_code == StandardErrorCode.AuthRequired)
                    errorCode = 401;

                //Create response to send
                await Program.QuickWriteJsonToDoc(e, new LibDeltaSystem.Entities.HttpErrorResponse
                {
                    message = ex.screen_error,
                    message_more = "Error of type "+ex.error_code.ToString(),
                    support_tag = null
                }, errorCode);
            } catch (Exception ex)
            {
                //Log this error with the system
                await Program.QuickWriteJsonToDoc(e, Program.connection.LogHttpError(ex, new Dictionary<string, string>()).GetAwaiter().GetResult(), 500);
            }
        }
    }
}
