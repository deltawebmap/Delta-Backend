using ArkWebMapMasterServer.NetEntities;
using ArkWebMapMasterServer.PresistEntities;
using ArkWebMapMasterServer.Users;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Auth
{
    public class AuthHttpHandler
    {
        public static Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, string path)
        {
            //Require POST
            if (Program.FindRequestMethod(e) != RequestHttpMethod.post)
                throw new StandardError("Method can only be post.", StandardErrorCode.NotFound);

            //Check path
            if (path.StartsWith("password/create"))
            {
                //Create user.
                return OnPasswordCreate(e);
            }
            if (path.StartsWith("password/login"))
            {
                //Create user.
                return OnPasswordLogin(e);
            }

            //Not found
            throw new StandardError("Not Found", StandardErrorCode.NotFound);
        }

        //User is using username/password and would like to create a user.
        private static Task OnPasswordCreate(Microsoft.AspNetCore.Http.HttpContext e)
        {
            //Decode payload
            CreateUserWithUsernamePasswordPayload payload = Program.DecodePostBody<CreateUserWithUsernamePasswordPayload>(e);

            //If payload is null, stop
            if (payload.password == null || payload.username == null)
                throw new StandardError("Missing password or username.", StandardErrorCode.MissingRequiredArg);

            //Create user
            ArkUser u = UserAuth.CreateUserWithUsernameAndPassword(payload.username, payload.password);

            //Pass on
            if(u == null)
                return OnFinishUserAuth(e, u, "That username is either already in use, or is greater than 24 characters or less than 4.");
            else
                return OnFinishUserAuth(e, u, "Created user.");
        }

        //Handles login requests
        private static Task OnPasswordLogin(Microsoft.AspNetCore.Http.HttpContext e)
        {
            //Decode payload
            CreateUserWithUsernamePasswordPayload payload = Program.DecodePostBody<CreateUserWithUsernamePasswordPayload>(e);

            //If payload is null, stop
            if (payload.password == null || payload.username == null)
                throw new StandardError("Missing password or username.", StandardErrorCode.MissingRequiredArg);

            //Authenticate
            ArkUser u = UserAuth.SignInUserWithUsernamePassword(payload.username, payload.password);

            //Pass on
            if (u == null)
                return OnFinishUserAuth(e, u, "Invalid username or password.");
            else
                return OnFinishUserAuth(e, u, "Signed in.");
        }

        //Called once the user is authenticated using whatever method.
        public static Task OnFinishUserAuth(Microsoft.AspNetCore.Http.HttpContext e, ArkUser u, string message)
        {
            //Generate reply
            AuthReply reply = new AuthReply
            {
                ok = u != null,
                message = message,
                user = u
            };

            if(u != null)
            {
                //If the user was authenticated, generate a token for them.
                string token = UserTokens.GenerateUserToken(u);

                //Set browser cookie
                e.Response.Cookies.Append("user_token", token, new Microsoft.AspNetCore.Http.CookieOptions
                {
                    IsEssential = true,
                    Expires = DateTime.UtcNow.AddYears(1),
                    Path = "/",
                    SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict
                });
            }

            //Write reply
            return Program.QuickWriteJsonToDoc(e, reply);
        }
    }
}
