using LibDeltaSystem;
using LibDeltaSystem.Db.System;
using LibDeltaSystem.WebFramework.ServiceTemplates;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static ArkWebMapMasterServer.Services.Auth.OAuth.OAuthScopeStatics;

namespace ArkWebMapMasterServer.Services.Auth.OAuth
{
    public class OAuthAuthorize : UserAuthDeltaService
    {
        public OAuthAuthorize(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        /// <summary>
        /// Starts the authentication process by sending the user to Steam authentication if requested
        /// If the user is already authenticated, this redirects to the application's domain immediately
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public override async Task OnRequest()
        {
            //Decode request
            OAuthInfoRequest request = await DecodePOSTBody<OAuthInfoRequest>();

            //Find the application
            DbOauthApp app = await Program.connection.GetOAuthAppByAppID(request.client_id);
            if (app == null)
            {
                await WriteString("Application ID invalid.", "text/plain", 400);
                return;
            }

            //Get a list of scopes used
            ulong scope = ulong.Parse(request.scopes);

            //Send to the application
            //Authenticated. Go now
            await SendToApplication(app, scope);
        }

        /// <summary>
        /// Creates an OAuth token and redirects to the application
        /// </summary>
        /// <returns></returns>
        public async Task SendToApplication(DbOauthApp app, ulong scope)
        {
            //Create a token
            var token = await user.MakeOAuthToken(Program.connection, app, scope);

            //Send response
            await WriteJSON(new ResponseData
            {
                next_url = app.redirect_uri + "?oauth_token=" + token.oauth_preflight
            });
        }

        public override async Task<bool> SetArgs(Dictionary<string, string> args)
        {
            return true;
        }

        class OAuthInfoRequest
        {
            public string client_id;
            public string scopes;
        }

        class ResponseData
        {
            public string next_url;
        }
    }
}
