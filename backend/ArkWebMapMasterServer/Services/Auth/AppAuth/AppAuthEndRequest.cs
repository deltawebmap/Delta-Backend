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
                await Program.QuickWriteToDoc(e, "Failed to sign in, you might have taken too long.", "text/plain", 400); //TODO: REDIRECT BACK TO LOGIN
                return;
            }

            //Do auth
            DbUser user = await SteamAuth.SteamOpenID.Finish(e);
            if (user == null)
            {
                await Program.QuickWriteToDoc(e, "Failed to sign in. Try again.", "text/plain", 400); //TODO: REDIRECT BACK TO LOGIN
                return;
            }

            //Update
            await session.SetUser(Program.connection, user);

            //Redirect to final endpoint
            string url = PREFLIGHT_OUT_URLS[session.redirect_type].Replace("{STATE}", state);
            e.Response.Headers.Add("Location", url);
            await Program.QuickWriteToDoc(e, "Redirecting...", "text/plain", 302);
        }
    }
}
