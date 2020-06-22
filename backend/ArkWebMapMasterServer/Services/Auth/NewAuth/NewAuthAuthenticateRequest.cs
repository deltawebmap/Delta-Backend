using LibDeltaSystem;
using LibDeltaSystem.Db.System;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Auth.NewAuth
{
    public class NewAuthAuthenticateRequest : INewAuthService
    {
        public NewAuthAuthenticateRequest(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public override async Task OnRequest()
        {
            //Grab params from URL
            string deltaToken = e.Request.Query["token"];
            string deltaNonce = e.Request.Query["nonce"];
            string claimedId = e.Request.Query["openid.claimed_id"];
            string identity = e.Request.Query["openid.identity"];
            string openIdNonce = e.Request.Query["openid.response_nonce"];
            string openIdHandle = e.Request.Query["openid.assoc_handle"];
            string openIdSig = e.Request.Query["openid.sig"];

            //Make sure none of the params are missing
            if(deltaToken == null || deltaNonce == null || claimedId == null || identity == null || openIdNonce == null || openIdHandle == null || openIdSig == null)
            {
                await WriteString("Missing required argument.", "text/plain", 400);
                return;
            }

            //Get session
            var session = await GetAuthSessionAsync(deltaToken);
            if(session == null)
            {
                await WriteString("Session ID not found. Did it expire?", "text/plain", 400);
                return;
            }
            if(session.nonce != deltaNonce)
            {
                await WriteString("Nonce incorrect. Try again.", "text/plain", 400);
                return;
            }
            if (session.state != DbAuthenticationSession.AuthState.PendingExternalAuth)
            {
                await WriteString("This session is in the incorrect state.", "text/plain", 400);
                return;
            }

            //Create the original URL we used to access this, as it's important
            string requestUrl = $"{Program.connection.config.hosts.master}/api/auth/authenticate?token={session.session_token}&nonce={session.nonce}";

            //Take in parameters from the URL to produce the outgoing validity one
            string validityUrl = $"https://steamcommunity.com/openid/login?openid.ns=http%3A%2F%2Fspecs.openid.net%2Fauth%2F2.0&openid.mode=check_authentication&openid.op_endpoint=https%3A%2F%2Fsteamcommunity.com%2Fopenid%2Flogin&openid.claimed_id={System.Web.HttpUtility.UrlEncode(claimedId)}&openid.identity={System.Web.HttpUtility.UrlEncode(identity)}&openid.return_to={System.Web.HttpUtility.UrlEncode(requestUrl)}&openid.response_nonce={System.Web.HttpUtility.UrlEncode(openIdNonce)}&openid.assoc_handle={System.Web.HttpUtility.UrlEncode(openIdHandle)}&openid.signed=signed%2Cop_endpoint%2Cclaimed_id%2Cidentity%2Creturn_to%2Cresponse_nonce%2Cassoc_handle&openid.sig={System.Web.HttpUtility.UrlEncode(openIdSig)}";
            Console.WriteLine(validityUrl);

            //Request steam valididty
            string validation_return;
            try
            {
                using (WebClient hc = new WebClient())
                    validation_return = hc.DownloadString(validityUrl);
            }
            catch
            {
                await WriteString("Steam auth failed. Try again. (STEAM_HTTP_FAIL)", "text/plain", 400);
                return;
            }

            //Return validation is really gross. We're just going to use a find.
            bool validationOk = validation_return.Contains("is_valid:true");
            if(!validationOk)
            {
                await WriteString("Steam auth failed. Try again. (STEAM_NOT_VALID)", "text/plain", 400);
                return;
            }

            //Now, we have their ID and have validated it. Extract it from the URL.
            string steam_id = claimedId.Substring("https://steamcommunity.com/openid/id/".Length);

            //Request this users' Steam profile.
            var profile = await conn.GetSteamProfileById(steam_id);
            if (profile == null)
            {
                await WriteString("Steam auth failed. Try again. (STEAM_PROFILE_FAIL)", "text/plain", 400);
                return;
            }

            //Get user account
            DbUser user = await DbUser.GetUserBySteamID(conn, profile);

            //Update session
            session.state = DbAuthenticationSession.AuthState.PendingOauthAuth;
            session.custom_data.Add("user_id", user.id);
            await session.UpdateAsync(conn);

            //Get oauth app
            var app = await conn.GetOAuthAppByInternalID(session.application_id);
            //TODO: Do oauth

            //Create HTML
            HtmlData hData = new HtmlData
            {
                nonce = session.nonce,
                reject_url = $"/auth/?client_id="+app.client_id,
                beta_key_url = "/api/auth/validate_beta_key",
                return_url = app.redirect_uri,
                user_id = user.id
            };
            string html = $"<!DOCTYPE html><html id=\"html\"><head><meta charset=\"UTF-8\"><title>Delta Web Map - Login...</title><link href=\"https://fonts.googleapis.com/css?family=Roboto\" rel=\"stylesheet\"><link rel=\"stylesheet\" href=\"/assets/auth/auth.css\"><meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0, minimum-scale=1.0\"></head><body><div class=\"content\"><img class=\"welcomebox_icon\" src=\"{System.Web.HttpUtility.HtmlEncode(user.profile_image_url)}\" /><div class=\"welcomebox_text\">Welcome, <span class=\"welcomebox_name\">{System.Web.HttpUtility.HtmlEncode(user.screen_name)}</span>!</div></div><script>var DELTA_LOGON_DATA = {JsonConvert.SerializeObject(hData)};</script><script src=\"/assets/auth/auth_finished.js\"></script></body></html>";

            //Write
            await WriteString(html, "text/html", 200);
        }

        class HtmlData
        {
            public string nonce;
            public string reject_url;
            public string return_url;
            public string user_id;
            public string beta_key_url;
        }
    }
}
