using ArkBridgeSharedEntities.Entities;
using ArkWebMapMasterServer.NetEntities;
using ArkWebMapMasterServer.PresistEntities;
using ArkWebMapMasterServer.Users;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static ArkWebMapMasterServer.SteamAuth.SteamOpenID;

namespace ArkWebMapMasterServer.Services.Auth
{
    public class AuthHttpHandler
    {
        public static Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, string path)
        {
            //Check path
            /*if (path.StartsWith("password/create"))
            {
                //Require POST
                if (Program.FindRequestMethod(e) != RequestHttpMethod.post)
                    throw new StandardError("Method can only be post.", StandardErrorCode.NotFound);

                //Create user.
                return OnPasswordCreate(e);
            }
            if (path.StartsWith("password/login"))
            {
                //Require POST
                if (Program.FindRequestMethod(e) != RequestHttpMethod.post)
                    throw new StandardError("Method can only be post.", StandardErrorCode.NotFound);

                //Create user.
                return OnPasswordLogin(e);
            }*/
            if (path.StartsWith("steam_auth_return"))
            {
                //Handle
                return OnSteamReturnRequest(e);
            }
            if (path.StartsWith("steam_auth"))
            {
                //Get mode, if any
                string mode = "Web";
                if (e.Request.Query.ContainsKey("mode"))
                    mode = e.Request.Query["mode"];
                
                //Redirect to Steam auth
                string url = SteamAuth.SteamOpenID.Begin(mode);
                e.Response.Headers.Add("Location", url);
                return Program.QuickWriteToDoc(e, "", "text/plain", 302);
            }
            if(path.StartsWith("validate_preflight_token"))
            {
                //Return the user reply.
                string id = e.Request.Query["id"];
                if(url_tokens.ContainsKey(id))
                {
                    //Respond with this. Then, delete it.
                    Task responseTask = Program.QuickWriteJsonToDoc(e, url_tokens[id]);
                    url_tokens.Remove(id);
                    return responseTask;
                } else
                {
                    //Not found
                    throw new StandardError("Preflight ID not found.", StandardErrorCode.NotFound);
                }
            }
            

            //Not found
            throw new StandardError("Not Found", StandardErrorCode.NotFound);
        }

        private static Task OnSteamReturnRequest(Microsoft.AspNetCore.Http.HttpContext e)
        {
            //Finish Steam auth
            var info = SteamAuth.SteamOpenID.Finish(e);

            //Get user. If a user account isn't created yet, make one.
            ArkUser user = UserAuth.GetUserByAuthName(info.steam_id);
            if(user == null)
            {
                user = UserAuth.CreateUserWithSteam(info.steam_id, info.profile);
            }

            //Update profile
            user.screen_name = info.profile.personaname;
            user.profile_image_url = info.profile.avatarfull;
            user.is_steam_verified = true;
            user.steam_id = info.steam_id;
            user.Update();

            //Pass into the next method
            return OnFinishUserAuth(e, user, "Loaded from Steam profile.", true);
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


        public static void SetAuthCookie(Microsoft.AspNetCore.Http.HttpContext e, string token)
        {
            e.Response.Cookies.Append("user_token", token, new Microsoft.AspNetCore.Http.CookieOptions
            {
                IsEssential = true,
                Expires = DateTime.UtcNow.AddYears(1),
                Path = "/",
                SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict
            });
        }

        /// <summary>
        /// One-time tokens used to get the real user token.
        /// </summary>
        public static Dictionary<string, AuthReply> url_tokens = new Dictionary<string, AuthReply>();

        //Called once the user is authenticated using whatever method.
        public static Task OnFinishUserAuth(Microsoft.AspNetCore.Http.HttpContext e, ArkUser u, string message, bool redirect = false)
        {
            //If the user was authenticated, set a token.
            string token = null;
            if(u != null)
            {
                token = UserTokens.GenerateUserToken(u);
            }

            //Generate reply
            AuthReply reply = new AuthReply
            {
                ok = u != null,
                message = message,
                user = u,
                token = token
            };

            //If the mode is set to one of the mobile clients, awake the app.
            SteamAuthMode mode = Enum.Parse<SteamAuthMode>(e.Request.Query["mode"]);
            if (mode == SteamAuthMode.AndroidClient)
            {
                //Create a temporary token the client can use to lookup the user.
                string id = Program.GenerateRandomString(24);
                while (url_tokens.ContainsKey(id))
                    id = Program.GenerateRandomString(24);

                //Create token and add.
                url_tokens.Add(id, reply);

                //Redirect back to the app.
                e.Response.Headers.Add("Location", "ark-web-map-login://login/" + id);
                return Program.QuickWriteToDoc(e, "You should be redirected back to the app. If not, let me know.", "text/plain", 302);
            }

            if (u != null)
            {
                //Set browser cookie
                SetAuthCookie(e, token);
            }

            //Write reply
            if(redirect)
            {
                //Redirect here
                e.Response.Headers.Add("Location", "https://ark.romanport.com/");
                return Program.QuickWriteToDoc(e, "You should be redirected home now.", "text/plain", 302);
            } else
            {
                //Just write for REST
                return Program.QuickWriteJsonToDoc(e, reply);
            }
        }
    }
}
