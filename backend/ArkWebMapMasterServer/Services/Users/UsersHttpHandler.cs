using ArkBridgeSharedEntities.Entities;
using ArkWebMapMasterServer.PresistEntities;
using LibDeltaSystem.Db.System;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Users
{
    public class UsersHttpHandler
    {
        public static DbUser AuthenticateUser(Microsoft.AspNetCore.Http.HttpContext e, bool required)
        {
            return AuthenticateUser(e, required, out string t);
        }

        public static string GetAuthToken(Microsoft.AspNetCore.Http.HttpContext e)
        {
            string token = null;
            if (e.Request.Headers.ContainsKey("Authorization"))
            {
                //Read Authorization header
                token = e.Request.Headers["Authorization"];
                if (token.StartsWith("Bearer "))
                    token = token.Substring("Bearer ".Length);
            }
            return token;
        }

        public static DbUser AuthenticateUser(Microsoft.AspNetCore.Http.HttpContext e, bool required, out string token)
        {
            token = GetAuthToken(e);
            if (token == null && required)
                throw new StandardError("No Auth Token Provided.", StandardErrorCode.AuthRequired);
            DbUser user = ArkWebMapMasterServer.Users.UserTokens.ValidateUserToken(token);
            if (user == null && required)
                throw new StandardError("You're not signed in.", StandardErrorCode.AuthRequired);
            return user;
        }

        public static Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, string path)
        {
            //Check if this is a demo user
            if(GetAuthToken(e) == "demo-user")
            {
                //This is a demo user. ALWAYS provide it with the placeholder users/me
                NetEntities.UsersMeReply usersme = new NetEntities.UsersMeReply();
                usersme.MakeDummyUsersMe();
                return Program.QuickWriteJsonToDoc(e, usersme);
            }

            //Get method
            var method = Program.FindRequestMethod(e);

            //Every path here requies authentication. Do it.
            DbUser user = AuthenticateUser(e, true, out string userToken);

            //Check path
            if (path == "@me/report_issue")
            {
                IssueCreator.OnHttpRequest(e, user).GetAwaiter().GetResult();
                return null;
            }
            if (path == "@me/tokens/@this/devalidate")
            {
                return TokenDevalidateService.OnSingleDevalidate(e, user, userToken);
            }
            if (path == "@me/tokens/@all/devalidate")
            {
                return TokenDevalidateService.OnAllDevalidate(e, user);
            }
            if(path == "@me/user_settings")
            {
                //Verify method
                if (Program.FindRequestMethod(e) != RequestHttpMethod.post)
                    throw new StandardError("Only POST or post requests are valid here.", StandardErrorCode.BadMethod);
                
                //Update
                user.user_settings = Program.DecodePostBody<DbUserSettings>(e);
                if(user.user_settings == null)
                {
                    throw new StandardError("Cannot set user settings to null.", StandardErrorCode.InvalidInput);
                }
                user.UpdateAsync().GetAwaiter().GetResult();
                return Program.QuickWriteStatusToDoc(e, true);
            }
            if(path == "@me/archive")
            {
                return UserDataDownloader.OnCreateRequest(e, user, userToken);
            }
            if (path == "@me/delete")
            {
                return UserDataRemover.OnHttpRequest(e, user, userToken);
            }
            if(path == "@me/create_machine" && method == RequestHttpMethod.post)
            {
                return Machines.CreateMachineRequest.OnUserCreateMachine(e, user);
            }
            if (path == "@me/machines")
            {
                return UsersMe.OnMachineListRequest(e, user);
            }
            if (path == "@me/push_token" && method == RequestHttpMethod.post)
            {
                return NotificationTokenRequest.OnHttpRequest(e, user);
            }
            if (path == "@me/" || path == "@me")
            {
                //Requested user info
                return UsersMe.OnHttpRequest(e, user);
            }

            //Not found
            throw new StandardError("Not Found", StandardErrorCode.NotFound);
        }
    }
}
