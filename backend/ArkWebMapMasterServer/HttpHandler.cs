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
                if (path.StartsWith("/clusters/"))
                {
                    //Pass onto this part
                    return Services.Clusters.ClustersHttpHandler.OnHttpRequest(e, path.Substring("/clusters/".Length));
                }
                if (path.StartsWith("/bridge/"))
                {
                    //Pass onto this part
                    return Services.Bridge.BridgeHttpHandler.OnHttpRequest(e, path.Substring("/bridge/".Length));
                }
                if (path.StartsWith("/auth/"))
                {
                    //Pass onto this part
                    return Services.Auth.AuthHttpHandler.OnHttpRequest(e, path.Substring("/auth/".Length));
                }
                if (path.StartsWith("/providers/"))
                {
                    //Pass onto this part
                    return Services.Providers.ProvidersHttpHandler.OnHttpRequest(e, path.Substring("/providers/".Length));
                }
                if (path.StartsWith("/server_setup_proxy/"))
                {
                    //This is the setup proxy for communicating with up-and-coming servers.
                    return Services.Misc.ArkSetupProxy.OnSetupProxyHttpRequest(e, path.Substring("/server_setup_proxy/".Length));
                }
                if (path.StartsWith("/server_validation"))
                {
                    //This is the setup proxy for communicating with up-and-coming servers.
                    return Services.Misc.ServerValidation.OnHttpRequest(e);
                }
                if (path.StartsWith("/system_server_validation"))
                {
                    //This is the setup proxy for communicating with up-and-coming servers.
                    return Services.Misc.SystemServerValidation.OnHttpRequest(e);
                }
                if (path.StartsWith("/obtain_server_setup_proxy_code"))
                {
                    //This is the setup proxy for communicating with up-and-coming servers.
                    return Services.Misc.ArkSetupProxy.OnObtainCode(e);
                }
                if (path.StartsWith("/mobile_login_code/"))
                {
                    //This is the setup proxy for communicating with up-and-coming servers.
                    return Services.Misc.MobileLoginTokenProxy.OnHttpRequest(e);
                }
                if (path.StartsWith("/discover/"))
                {
                    //ARK interface from an Ark server. Take care.
                    return Services.ServerDiscovery.ServerDiscoveryHttpHandler.OnHttpRequest(e, path.Substring("/discover/".Length));
                }
                if (path == "/mirror/events")
                {
                    //ARK interface from an Ark server. Take care.
                    return Services.Mirror.MirrorService.OnHttpRequest(e);
                }
                if (path == "/mirror/error")
                {
                    //ARK interface from an Ark server. Take care.
                    return Services.Mirror.MirrorErrorService.OnHttpRequest(e);
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

                //Unknown
                throw new StandardError("Not Found", StandardErrorCode.NotFound);
            } catch (StandardError ex)
            {
                //Log error
                ErrorLogger.LogStandardError(ex, e);
                
                //Write error
                int errorCode = 500;
                if (ex.error_code == StandardErrorCode.AuthRequired)
                    errorCode = 401;
                return Program.QuickWriteJsonToDoc(e, ex, errorCode);
            } catch (Exception ex)
            {
                //Log error
                ErrorLogger.LogException(ex, e);

                //Write error
                return Program.QuickWriteJsonToDoc(e, new StandardError(ex.Message + ex.StackTrace, StandardErrorCode.UncaughtException, ex), 500);
            }
        }

        
    }
}
