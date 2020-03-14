using LibDeltaSystem;
using LibDeltaSystem.Db.System;
using LibDeltaSystem.WebFramework.ServiceTemplates;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Auth.OAuth
{
    public class OAuthVerifyRequest : BasicDeltaService
    {
        public OAuthVerifyRequest(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        /// <summary>
        /// Used to obtain an access token from a backend server
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public override async Task OnRequest()
        {
            //Decode request
            VerifyRequestPayload request = await DecodePOSTBody<VerifyRequestPayload>();

            //Find the application
            DbOauthApp app = await Program.connection.GetOAuthAppByAppID(request.client_id);
            if (app == null)
                throw new StandardError("App not found.", StandardErrorCode.NotFound);

            //Verify that the secret matches
            if (request.client_secret != app.client_secret)
                throw new StandardError("Client secret does not match!", StandardErrorCode.InternalSigninError);

            //Get a token using this
            var token = await Program.connection.GetTokenByPreflightAsync(request.preflight_token);
            if (token == null)
            {
                await WriteJSON(new VerifyResponsePayload
                {
                    ok = false
                });
                return;
            }

            //Deactivate preflight token internally
            token.oauth_preflight = null;
            await token.UpdateAsync(Program.connection);

            //Create and write a response
            await WriteJSON(new VerifyResponsePayload
            {
                access_token = token.token,
                scopes = token.oauth_scopes,
                ok = true
            });
        }

        class VerifyRequestPayload
        {
            public string client_id;
            public string client_secret;
            public string preflight_token;
        }

        class VerifyResponsePayload
        {
            public bool ok;
            public string access_token;
            public string[] scopes;
        }
    }
}
