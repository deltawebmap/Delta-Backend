using ArkWebMapMasterServer.NetEntities;
using LibDeltaSystem.Db.System;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static ArkWebMapMasterServer.SteamAuth.SteamOpenID;

namespace ArkWebMapMasterServer.Services.Auth
{
    public class AuthHttpHandler
    {
        /// <summary>
        /// Holds one-time sessions 
        /// </summary>
        private static Dictionary<string, OneTimeLoginSession> oneTimeSessions = new Dictionary<string, OneTimeLoginSession>();

        public static async Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, string path)
        {
            if (path.StartsWith("steam_auth_return"))
            {
                //Finish Steam auth and let handler take care of the rest
                await SteamAuth.SteamOpenID.Finish(e);
                return;
            }
            if (path.StartsWith("steam_auth/"))
            {
                //We're doing auth. Determine the method
                string methodName = path.Substring("steam_auth/".Length);

                //Find the method
                OneTimeLoginSession session;
                switch(methodName)
                {
                    case "": session = new LoginSessionWeb(); break;
                    case "android_secure": session = new LoginSessionAndroid(); break;
                    default:
                        throw new StandardError("Unknown auth method.", StandardErrorCode.AuthFailed);
                }

                //Run intro
                session.OnOpen(e);

                //Redirect to Steam auth
                string url = SteamAuth.SteamOpenID.Begin(session);
                e.Response.Headers.Add("Location", url);
                await Program.QuickWriteToDoc(e, "Redirecting to STEAM authentication.", "text/plain", 302);
                return;
            }
            if(path.StartsWith("validate_preflight_token"))
            {
                //Handles one-time response

                //Get the data and destroy it from the dict
                if (!oneTimeSessions.ContainsKey(e.Request.Query["id"]))
                    throw new StandardError("Auth failed, this token expired, or it never existed.", StandardErrorCode.AuthFailed);
                OneTimeLoginSession data = oneTimeSessions[e.Request.Query["id"].ToString()];
                oneTimeSessions.Remove(e.Request.Query["id"]);

                //Create response
                OneTimeLoginResponse response = new OneTimeLoginResponse
                {
                    access_token = data.access_token,
                    extra = data.CreateResponseData()
                };

                //Write data
                await Program.QuickWriteJsonToDoc(e, response);
            }
            if(path.StartsWith("oauth/"))
            {
                await OAuth.OAuthHttpHandler.OnOAuthRequest(e, path.Substring("oauth".Length));
                return;
            }

            //Not found
            throw new StandardError("Not Found", StandardErrorCode.NotFound);
        }

        class OneTimeLoginResponse
        {
            public string access_token;
            public object extra;
        }

        /// <summary>
        /// Class used for auth methods that involve going back to our own page to set an access token. Used for official only.
        /// </summary>
        abstract class OneTimeLoginSession : SteamAuth.SteamOpenID.SteamOpenIDCallback
        {
            /// <summary>
            /// Response object is sent over HTTP to the pending body
            /// </summary>
            /// <returns></returns>
            public abstract object CreateResponseData();

            /// <summary>
            /// Token that can be used to request this session
            /// </summary>
            public readonly string one_time_token;

            /// <summary>
            /// The access token response
            /// </summary>
            public string access_token;

            /// <summary>
            /// Handles finished auth
            /// </summary>
            /// <param name="e"></param>
            /// <param name="profile"></param>
            /// <param name="user"></param>
            /// <returns></returns>
            public override async Task OnAuthFinished(HttpContext e, DbSteamCache profile, DbUser user)
            {
                //Create an access token
                access_token = await user.MakeToken();
            }

            /// <summary>
            /// Creates a one-time session. Automatically generates a token and attaches it
            /// </summary>
            public OneTimeLoginSession()
            {
                //Create token
                string id = Program.GenerateRandomString(56);
                while (AuthHttpHandler.oneTimeSessions.ContainsKey(id))
                    id = Program.GenerateRandomString(56);

                //Attach
                one_time_token = id;
                AuthHttpHandler.oneTimeSessions.Add(id, this);
            }

            public abstract void OnOpen(HttpContext e);
        }

        /// <summary>
        /// Session used for OFFICIAL login requests to web
        /// </summary>
        class LoginSessionWeb : OneTimeLoginSession
        {
            public string next_url;

            public override object CreateResponseData()
            {
                return next_url;
            }

            public override async Task OnAuthFinished(HttpContext e, DbSteamCache profile, DbUser user)
            {
                //Run base
                await base.OnAuthFinished(e, profile, user);

                //Create URL to redirect to
                string url = "https://deltamap.net/login/return/#" + one_time_token;

                //Redirect to this
                e.Response.Headers.Add("Location", url);
                await Program.QuickWriteToDoc(e, "You should be redirected now.", "text/plain", 302);
            }

            public LoginSessionWeb() : base()
            {

            }

            public override void OnOpen(HttpContext e)
            {
                //Get the next url
                string next = "https://deltamap.net/app/";
                if (e.Request.Query.ContainsKey("next"))
                    next = e.Request.Query["next"];
                next_url = next;
            }
        }

        /// <summary>
        /// Session used for OFFICIAL Android requests to web
        /// </summary>
        class LoginSessionAndroid : OneTimeLoginSession
        {
            DbUser user;

            public string nonce;

            public override object CreateResponseData()
            {
                return user;
            }

            public override async Task OnAuthFinished(HttpContext e, DbSteamCache profile, DbUser user)
            {
                //Run base
                await base.OnAuthFinished(e, profile, user);
                this.user = user;
                
                //Create URL to redirect to
                string url = "delta-web-map://login/" + nonce + "/" + one_time_token;

                //Redirect to this
                e.Response.Headers.Add("Location", url);
                await Program.QuickWriteToDoc(e, "You should be redirected now.", "text/plain", 302);
            }

            public LoginSessionAndroid() : base()
            {

            }

            public override void OnOpen(HttpContext e)
            {
                //Get the next url
                if (e.Request.Query.ContainsKey("nonce"))
                    nonce = e.Request.Query["nonce"];
            }
        }
    }
}
