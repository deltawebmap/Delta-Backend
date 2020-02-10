using LibDeltaSystem.Db.System;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static ArkWebMapMasterServer.Services.Auth.OAuth.OAuthScopeStatics;

namespace ArkWebMapMasterServer.Services.Auth.OAuth
{
    public static class OAuthAuthorize
    {
        /// <summary>
        /// Starts the authentication process by sending the user to Steam authentication if requested
        /// If the user is already authenticated, this redirects to the application's domain immediately
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static async Task OnAuthorizeRequest(Microsoft.AspNetCore.Http.HttpContext e)
        {
            //Attempt to authorize the user
            DbUser user = null;

            //Get the application
            DbOauthApp app = await Program.connection.GetOAuthAppByAppID(e.Request.Query["client_id"]);
            if (app == null)
                throw new StandardError("Application not found.", StandardErrorCode.NotFound);
            
            //Get a list of scopes used
            var scopes = OAuthScopeStatics.GetOAuthScopes(e.Request.Query["scopes"].ToString().Split(','));
            string[] scopeIDs = OAuthScopeStatics.GetOAuthScopeIDs(scopes, out bool is_dangerous);

            //Send to the application
            //Authenticated. Go now
            await SendToApplication(e, app, user, scopeIDs);
        }

        /// <summary>
        /// Creates an OAuth token and redirects to the application
        /// </summary>
        /// <returns></returns>
        public static async Task SendToApplication(Microsoft.AspNetCore.Http.HttpContext e, DbOauthApp app, DbUser user, string[] scopeIDs)
        {
            //Create a token
            var token = await user.MakeOAuthToken(Program.connection, app, scopeIDs);

            //Redirect to the application
            e.Response.Headers.Add("Location", app.redirect_uri + "?t=" + token.oauth_preflight);
            await Program.QuickWriteToDoc(e, "You should be redirected now.", "text/plain", 302);
        }

        
    }
}
