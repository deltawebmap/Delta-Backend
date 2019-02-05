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
            Console.WriteLine($"Got request to {e.Request.Path}");
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
                if(path.StartsWith("/invites/"))
                {
                    //This is an invite. Respond with it.
                    var invite = ArkWebMapMasterServer.Servers.ArkServerInviteManager.GetInviteById(path.Substring("/invites/".Length));
                    if (invite == null)
                        throw new StandardError("Invite not found", StandardErrorCode.NotFound);
                    return Program.QuickWriteJsonToDoc(e, new NetEntities.InviteReply(invite));
                }

                //Unknown
                throw new StandardError("Not Found", StandardErrorCode.NotFound);
            } catch (StandardError ex)
            {
                //Write error
                return Program.QuickWriteJsonToDoc(e, ex, 500);
            } catch (Exception ex)
            {
                //Write error
                return Program.QuickWriteJsonToDoc(e, new StandardError(ex.Message+ex.StackTrace, StandardErrorCode.UncaughtException, ex), 500);
            }
        }
    }
}
