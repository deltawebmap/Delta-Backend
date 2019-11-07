using ArkWebMapMasterServer.NetEntities;
using LibDeltaSystem.Db.System;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static ArkWebMapMasterServer.SteamAuth.SteamOpenID;

namespace ArkWebMapMasterServer.Services.Auth
{
    public class AuthHttpHandler
    {
        public static async Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, string path)
        {
            if (path.StartsWith("steam_auth_return"))
            {
                //Handle
                await OnSteamReturnRequest(e);
                return;
            }
            if (path.StartsWith("steam_auth"))
            {
                //Get mode, if any
                string mode = "Web";
                if (e.Request.Query.ContainsKey("mode"))
                    mode = e.Request.Query["mode"];

                //Get the next url
                string next = "https://deltamap.net/app/";
                if (e.Request.Query.ContainsKey("next"))
                    next = e.Request.Query["next"];

                //Redirect to Steam auth
                string url = SteamAuth.SteamOpenID.Begin(mode, next);
                e.Response.Headers.Add("Location", url);
                await Program.QuickWriteToDoc(e, "", "text/plain", 302);
                return;
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
                    await responseTask;
                    return;
                } else
                {
                    //Not found
                    throw new StandardError("Preflight ID not found.", StandardErrorCode.NotFound);
                }
            }

            //Not found
            throw new StandardError("Not Found", StandardErrorCode.NotFound);
        }

        private static async Task OnSteamReturnRequest(Microsoft.AspNetCore.Http.HttpContext e)
        {
            //Finish Steam auth
            var info = await SteamAuth.SteamOpenID.Finish(e);

            //Get user. If a user account isn't created yet, make one.
            DbUser user = await Program.connection.GetUserBySteamIdAsync(info.steam_id);
            if (user == null)
            {
                //Create the user
                user = new DbUser
                {
                    user_settings = new DbUserSettings(),
                    profile_image_url = info.profile.icon_url,
                    steam_profile_url = info.profile.profile_url,
                    screen_name = info.profile.name,
                    steam_id = info.profile.steam_id,
                    _id = MongoDB.Bson.ObjectId.GenerateNewId(),
                    conn = Program.connection
                };

                //Insert in the database
                await Program.connection.system_users.InsertOneAsync(user);
            }

            //Pass into the next method
            await OnFinishUserAuth(e, user, "Loaded from Steam profile.", info.next, true);
        }

        /// <summary>
        /// One-time tokens used to get the real user token.
        /// </summary>
        public static Dictionary<string, AuthReply> url_tokens = new Dictionary<string, AuthReply>();

        //Called once the user is authenticated using whatever method.
        public static async Task OnFinishUserAuth(Microsoft.AspNetCore.Http.HttpContext e, DbUser u, string message, string next, bool redirect = false)
        {
            //If the user was authenticated, set a token.
            string token = null;
            if(u != null)
            {
                token = await u.MakeToken();
            }

            //Generate reply
            AuthReply reply = new AuthReply
            {
                ok = u != null,
                message = message,
                user = u,
                token = token,
                next = next
            };

            //Create a temporary token the client can use to lookup the user.
            string id = Program.GenerateRandomString(24);
            while (url_tokens.ContainsKey(id))
                id = Program.GenerateRandomString(24);

            //Create token and add.
            url_tokens.Add(id, reply);

            //If the mode is set to one of the mobile clients, awake the app.
            SteamAuthMode mode = Enum.Parse<SteamAuthMode>(e.Request.Query["mode"]);
            if (mode == SteamAuthMode.AndroidClient)
            {
                //Redirect back to the app.
                e.Response.Headers.Add("Location", "ark-web-map-login://login/" + id);
                await Program.QuickWriteToDoc(e, "You should be redirected back to the app. If not, let me know.", "text/plain", 302);
                return;
            } else
            {
                //Redirect to the login page and pass in the token
                e.Response.Headers.Add("Location", "https://deltamap.net/login/return/#"+id);
                await Program.QuickWriteToDoc(e, "You should be redirected now.", "text/plain", 302);
                return;
            }
        }
    }
}
