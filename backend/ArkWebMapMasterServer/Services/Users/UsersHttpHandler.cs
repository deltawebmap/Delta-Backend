using ArkWebMapMasterServer.PresistEntities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Users
{
    public class UsersHttpHandler
    {
        public static ArkUser AuthenticateUser(Microsoft.AspNetCore.Http.HttpContext e, bool required)
        {
            ArkUser user = null;
            if (e.Request.Cookies.ContainsKey("user_token"))
            {
                string token = e.Request.Cookies["user_token"];
                user = ArkWebMapMasterServer.Users.UserTokens.ValidateUserToken(token);
            }
            if (user == null && required)
                throw new StandardError("You're not signed in.", StandardErrorCode.AuthRequired);
            return user;
        }

        public static Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, string path)
        {
            //Every path here requies authentication. Do it.
            ArkUser user = AuthenticateUser(e, true);

            //Check path
            if (path.StartsWith("create_server"))
            {
                //Pass onto server gen
                return ServerCreation.OnHttpRequest(e, user);
            }
            if (path.StartsWith("@me/invites/accept"))
            {
                //Requested user info
                return UsersMe.OnAcceptInviteRequest(e, user);
            }
            if (path.StartsWith("@me/"))
            {
                //Requested user info
                return UsersMe.OnHttpRequest(e, user);
            }

            //Not found
            throw new StandardError("Not Found", StandardErrorCode.NotFound);
        }
    }
}
