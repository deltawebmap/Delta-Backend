using LibDeltaSystem;
using LibDeltaSystem.Db.System;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Auth.AppAuth
{
    public class AppAuthTokenRequest : AppAuthRequestTemplate
    {
        public AppAuthTokenRequest(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public override async Task OnRequest()
        {
            //Get preflight token
            string state = e.Request.Query["state"];
            DbPreflightToken session = await Program.connection.GetPreflightTokenByTokenAsync(state);
            if (session == null)
            {
                await Program.QuickWriteToDoc(e, "Failed to sign in, you might have taken too long.", "text/plain", 400); //TODO: REDIRECT BACK TO LOGIN
                return;
            }

            //Verify
            if (!session.auth)
            {
                await Program.QuickWriteToDoc(e, "Token is not yet valid.", "text/plain", 400);
                return;
            }

            //Create output data
            TokenResponseData d = new TokenResponseData
            {
                token = session.final_token,
                next = session.next,
                nonce = session.nonce
            };

            await Program.QuickWriteJsonToDoc(e, d);
        }

        class TokenResponseData
        {
            public string token;
            public string next;
            public int nonce;
        }
    }
}
