using LibDeltaSystem;
using LibDeltaSystem.Db.System;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

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
            string deltaToken = e.Request.Query[URLPARAM_TOKEN];
            string deltaNonce = e.Request.Query[URLPARAM_NONCE];
            string claimedId = e.Request.Query["openid.claimed_id"];
            string identity = e.Request.Query["openid.identity"];
            string openIdNonce = e.Request.Query["openid.response_nonce"];
            string openIdHandle = e.Request.Query["openid.assoc_handle"];
            string openIdSig = e.Request.Query["openid.sig"];

            //Get retry data
            string[] retryData = null;
            if(e.Request.Query.ContainsKey(URLPARAM_RETRY))
            {
                //Attempt to deserialize
                try
                {
                    retryData = JsonConvert.DeserializeObject<string[]>(e.Request.Query[URLPARAM_RETRY]);
                    if (retryData.Length != 3)
                        retryData = null;
                } catch
                {
                    //Ignore
                    retryData = null;
                }
            }

            //Make sure none of the params are missing
            if(deltaToken == null || deltaNonce == null || claimedId == null || identity == null || openIdNonce == null || openIdHandle == null || openIdSig == null)
            {
                await WriteErrorResponse("Request Error", "Missing a required argument.", retryData);
                return;
            }

            //Get session
            var session = await GetAuthSessionAsync(deltaToken);
            if(session == null)
            {
                await WriteErrorResponse("Login Error", "Your login session has expired, or you're attempting to go back.", retryData);
                return;
            }
            if(session.nonce != deltaNonce)
            {
                await WriteErrorResponse("Security Error", "Nonce was invalid.", retryData);
                return;
            }
            if (session.state != DbAuthenticationSession.AuthState.PendingExternalAuth)
            {
                await WriteErrorResponse("Login Error", "Your login session is not in the correct state.", retryData);
                return;
            }

            //Create the original URL we used to access this, as it's important
            string requestUrl = $"{Program.connection.config.hosts.master}/api/auth/authenticate?{URLPARAM_TOKEN}={session.session_token}&{URLPARAM_NONCE}={session.nonce}&{URLPARAM_RETRY}={HttpUtility.UrlEncode(e.Request.Query[URLPARAM_RETRY])}";

            //Take in parameters from the URL to produce the outgoing validity one
            string validityUrl = $"https://steamcommunity.com/openid/login?openid.ns=http%3A%2F%2Fspecs.openid.net%2Fauth%2F2.0&openid.mode=check_authentication&openid.op_endpoint=https%3A%2F%2Fsteamcommunity.com%2Fopenid%2Flogin&openid.claimed_id={System.Web.HttpUtility.UrlEncode(claimedId)}&openid.identity={System.Web.HttpUtility.UrlEncode(identity)}&openid.return_to={System.Web.HttpUtility.UrlEncode(requestUrl)}&openid.response_nonce={System.Web.HttpUtility.UrlEncode(openIdNonce)}&openid.assoc_handle={System.Web.HttpUtility.UrlEncode(openIdHandle)}&openid.signed=signed%2Cop_endpoint%2Cclaimed_id%2Cidentity%2Creturn_to%2Cresponse_nonce%2Cassoc_handle&openid.sig={System.Web.HttpUtility.UrlEncode(openIdSig)}";

            //Request steam valididty
            string validation_return;
            try
            {
                using (WebClient hc = new WebClient())
                    validation_return = hc.DownloadString(validityUrl);
            }
            catch
            {
                conn.Log("DeltaAuth", $"Couldn't connect to Steam to finish authentication! SteamID={claimedId}", DeltaLogLevel.Alert);
                await WriteErrorResponse("Validation Error", "Couldn't connect to Steam to validate your authentication.", retryData);
                return;
            }

            //Return validation is really gross. We're just going to use a find.
            bool validationOk = validation_return.Contains("is_valid:true");
            if(!validationOk)
            {
                conn.Log("DeltaAuth", $"Steam authentication was invalid.", DeltaLogLevel.High);
                await WriteErrorResponse("Validation Error", "Steam validation error. (STEAM_NOT_VALID)", retryData);
                return;
            }

            //Now, we have their ID and have validated it. Extract it from the URL.
            string steam_id = claimedId.Substring("https://steamcommunity.com/openid/id/".Length);

            //Request this users' Steam profile.
            var profile = await conn.GetSteamProfileById(steam_id);
            if (profile == null)
            {
                conn.Log("DeltaAuth", $"Steam authentication passed, but no Steam profile was found! SteamID={claimedId}", DeltaLogLevel.High);
                await WriteErrorResponse("Validation Error", "Steam validation error. (STEAM_NO_PROFILE)", retryData);
                return;
            }

            //Get user account
            DbUser user = await DbUser.GetUserBySteamID(conn, profile);

            //Update session
            session.state = DbAuthenticationSession.AuthState.PendingOauthAuth;
            session.custom_data.Add(CUSTOM_DATA_KEY__USER_ID, user.id);
            await session.UpdateAsync(conn);

            //Get oauth app
            var app = await conn.GetOAuthAppByInternalID(session.application_id);

            //Create oauth url
            string output = app.redirect_uri + "?oauth_token=" + session.session_token + "&oauth_custom=" + System.Web.HttpUtility.UrlEncode(session.custom_data[CUSTOM_DATA_KEY__OAUTH_CUSTOM_DATA]);

            //Create HTML
            HtmlData hData = new HtmlData
            {
                nonce = session.nonce,
                reject_url = $"/auth/?client_id="+app.client_id+"&scope="+session.scope,
                beta_key_url = "/api/auth/validate_beta_key",
                return_url = output,
                user_id = user.id
            };

            //Write
            await _WriteHTMLResponse($"<div id=\"og_content\" class=\"content\"><img class=\"welcomebox_icon\" src=\"{System.Web.HttpUtility.HtmlEncode(user.profile_image_url)}\" /><div class=\"welcomebox_text\">Welcome, <span class=\"welcomebox_name\">{System.Web.HttpUtility.HtmlEncode(user.screen_name)}</span>!</div></div><script>var DELTA_LOGON_DATA = {JsonConvert.SerializeObject(hData)};</script><script src=\"/assets/auth/auth_finished.js\"></script>");
        }

        private async Task _WriteHTMLResponse(string body, int status = 200)
        {
            await WriteString($"<!DOCTYPE html><html id=\"html\"><head><meta charset=\"UTF-8\"><title>Delta Web Map - Login...</title><link href=\"https://fonts.googleapis.com/css?family=Roboto\" rel=\"stylesheet\"><link rel=\"stylesheet\" href=\"/assets/auth/auth.css\"><meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0, minimum-scale=1.0\"></head><body>{body}<div class=\"logo_bottom\">DeltaWebMap</div></body></html>", "text/html", status);
        }

        private async Task WriteErrorResponse(string title, string body, string[] retryData)
        {
            string actions = "";
            if (retryData != null)
                actions += $"Please <a style=\"color: #3882dc;\" href=\"/auth/?client_id={HttpUtility.UrlEncode(retryData[0])}&scope={HttpUtility.UrlEncode(retryData[1])}&payload={HttpUtility.UrlEncode(retryData[2])}\">try again</a>.";
            await _WriteHTMLResponse($"<div class=\"content auth_content\"><div class=\"auth_fail_title\">{title}</div><div>{body} {actions}</div></div>", 400);
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
