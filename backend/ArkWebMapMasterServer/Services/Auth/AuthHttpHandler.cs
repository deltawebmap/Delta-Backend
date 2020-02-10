using ArkWebMapMasterServer.NetEntities;
using LibDeltaSystem.Db.System;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static ArkWebMapMasterServer.SteamAuth.SteamOpenID;

namespace ArkWebMapMasterServer.Services.Auth
{
    public class AuthHttpHandler
    {
        public static readonly string[] PREFLIGHT_OUT_URLS = new string[]
        {
            "https://deltamap.net/login/return/?state={STATE}",
            "https://dev.deltamap.net/login/return/?state={STATE}"
        };
        
        public static async Task OnBeginRequest(Microsoft.AspNetCore.Http.HttpContext e)
        {
            //Get args
            if(!e.Request.Query.ContainsKey("nonce") || !e.Request.Query.ContainsKey("next") || !e.Request.Query.ContainsKey("type"))
            {
                await Program.QuickWriteToDoc(e, "Missing Required Args (client out of date?)", "text/plain", 400);
                return;
            }
            if(!int.TryParse(e.Request.Query["nonce"], out int nonce))
            {
                await Program.QuickWriteToDoc(e, "Invalid Nonce", "text/plain", 400);
                return;
            }
            if (!int.TryParse(e.Request.Query["type"], out int type))
            {
                await Program.QuickWriteToDoc(e, "Invalid Type", "text/plain", 400);
                return;
            }
            string next = e.Request.Query["next"];

            //Create preflight token
            string session = LibDeltaSystem.Tools.SecureStringTool.GenerateSecureString(24);

            //Create preflight token
            DbPreflightToken t = new DbPreflightToken
            {
                redirect_type = type,
                next = next,
                auth = false,
                creation = DateTime.UtcNow,
                nonce = nonce,
                preflight_token = session
            };
            await Program.connection.system_preflight_tokens.InsertOneAsync(t);
            
            //Redirect to Steam auth
            string url = SteamAuth.SteamOpenID.Begin(session);
            e.Response.Headers.Add("Location", url);
            await Program.QuickWriteToDoc(e, "Redirecting to STEAM authentication.", "text/plain", 302);
        }

        public static async Task OnEndRequest(Microsoft.AspNetCore.Http.HttpContext e)
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

        public static async Task OnTokenRequest(Microsoft.AspNetCore.Http.HttpContext e)
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
            if(!session.auth)
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
