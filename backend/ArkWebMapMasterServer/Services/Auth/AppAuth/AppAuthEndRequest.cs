using LibDeltaSystem;
using LibDeltaSystem.Db.System;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Auth.AppAuth
{
    public class AppAuthEndRequest : AppAuthRequestTemplate
    {
        public AppAuthEndRequest(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public override async Task OnRequest()
        {
            //Get preflight token
            string state = e.Request.Query["state"];
            DbPreflightToken session = await Program.connection.GetPreflightTokenByTokenAsync(state);
            if (session == null)
            {
                await WriteString("Failed to sign in, you might have taken too long.", "text/plain", 400); //TODO: REDIRECT BACK TO LOGIN
                return;
            }

            //Do auth
            DbUser user = await SteamAuth.SteamOpenID.Finish(e);
            if (user == null)
            {
                await WriteString("Failed to sign in. Try again.", "text/plain", 400); //TODO: REDIRECT BACK TO LOGIN
                return;
            }

            //Validate and claim token
            if(!await conn.ValidateAndClaimBetaKey(session.custom_data["BETA_KEY"], user._id))
            {
                await WriteString($"<h1>Beta Key Invalid</h1><p>Oops! That beta key has already been used by a different user or is invalid.</p><p><u>USER ID:</u> {user.id}<br><u>BETA KEY:</u> {session.custom_data["BETA_KEY"]}</p><br><br><a href=\"/login/\">Try Again</a>", "text/html", 400);
                return;
            }

            //Update
            await session.SetUser(Program.connection, user);

            //Redirect to final endpoint
            string url = PREFLIGHT_OUT_URLS[session.redirect_type].Replace("{STATE}", state);
            e.Response.Headers.Add("Location", url);
            await WriteString("Redirecting...", "text/plain", 302);
        }
    }
}
