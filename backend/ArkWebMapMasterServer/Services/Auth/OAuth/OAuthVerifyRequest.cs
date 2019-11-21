using LibDeltaSystem.Db.System;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Auth.OAuth
{
    public static class OAuthVerifyRequest
    {
        /// <summary>
        /// Used to obtain an access token from a backend server
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static async Task OnVerifyRequest(Microsoft.AspNetCore.Http.HttpContext e)
        {
            //Decode request
            VerifyRequestPayload request = Program.DecodePostBody<VerifyRequestPayload>(e);

            //Find the application
            DbOauthApp app = await Program.connection.GetOAuthAppByAppID(request.client_id);
            if (app == null)
                throw new StandardError("App not found.", StandardErrorCode.NotFound);

            //Verify that the secret matches
            if (request.client_secret != app.client_secret)
                throw new StandardError("Client secret does not match!", StandardErrorCode.InternalSigninError);

            //Get a token using this
            var token = await Program.connection.GetTokenByPreflightAsync(request.preflight_token);
            if(token == null)
            {
                await Program.QuickWriteJsonToDoc(e, new VerifyResponsePayload
                {
                    ok = false
                });
                return;
            }

            //Deactivate preflight token internally
            token.oauth_preflight = null;
            await token.UpdateAsync();

            //Create and write a response
            await Program.QuickWriteJsonToDoc(e, new VerifyResponsePayload
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
