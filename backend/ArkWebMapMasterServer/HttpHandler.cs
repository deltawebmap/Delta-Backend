using ArkBridgeSharedEntities.Entities;
using ArkWebMapMasterServer.NetEntities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer
{
    public class HttpHandler
    {
        public static Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e)
        {
            Program.Log($"[Incoming request] {e.Request.Path}{e.Request.QueryString}", ConsoleColor.Blue);
            try
            {
                //Read the first part of the path.
                string path = e.Request.Path.ToString();
                if (path.StartsWith("/users/"))
                {
                    //Pass onto this part
                    return Services.Users.UsersHttpHandler.OnHttpRequest(e, path.Substring("/users/".Length));
                }
                if (path.StartsWith("/servers/"))
                {
                    //Pass onto this part
                    return Services.Servers.ServersHttpHandler.OnHttpRequest(e, path.Substring("/servers/".Length));
                }
                if (path.StartsWith("/machines/"))
                {
                    //Pass onto this part
                    return Services.Machines.MachinesHttpHandler.OnHttpRequest(e, path.Substring("/machines/".Length));
                }
                if (path.StartsWith("/auth/"))
                {
                    //Pass onto this part
                    return Services.Auth.AuthHttpHandler.OnHttpRequest(e, path.Substring("/auth/".Length));
                }
                if (path.StartsWith("/mobile_login_code/"))
                {
                    //This is the setup proxy for communicating with up-and-coming servers.
                    return Services.Misc.MobileLoginTokenProxy.OnHttpRequest(e);
                }
                if (path == "/status")
                {
                    //System status report
                    return Services.Misc.ServiceStatus.OnHttpRequest(e);
                }
                if (path == "/download_token")
                {
                    //File download
                    return Tools.TokenFileDownloadTool.OnDownloadRequest(e);
                }
                if(path == "/preregister")
                {
                    return Services.Misc.PreregisterUser.OnHttpRequest(e);
                }
                if (path == "/activate_machine")
                {
                    //First-time machine activation
                    return Services.Machines.MachineActivateRequest.OnActivateRequest(e);
                }

                //Unknown
                throw new StandardError("Not Found", StandardErrorCode.NotFound);
            } catch (StandardError ex)
            {
                //Set status code
                int errorCode = 500;
                if (ex.error_code == StandardErrorCode.AuthRequired)
                    errorCode = 401;

                //Create response to send
                return Program.QuickWriteJsonToDoc(e, new LibDeltaSystem.Entities.HttpErrorResponse
                {
                    message = ex.screen_error,
                    message_more = "Error of type "+ex.error_code.ToString(),
                    support_tag = null
                }, errorCode);
            } catch (Exception ex)
            {
                //Log this error with the system
                try
                {
                    return Program.QuickWriteJsonToDoc(e, Program.connection.LogHttpError(ex, new Dictionary<string, string>()).GetAwaiter().GetResult(), 500);
                } catch (Exception exx)
                {
                    Console.WriteLine(exx.Message + exx.StackTrace);
                }
                return null;
            }
        }
    }
}
