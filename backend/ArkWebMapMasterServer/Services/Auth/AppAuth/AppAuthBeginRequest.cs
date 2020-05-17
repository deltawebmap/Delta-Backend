﻿using LibDeltaSystem;
using LibDeltaSystem.Db.System;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Auth.AppAuth
{
    public class AppAuthBeginRequest : AppAuthRequestTemplate
    {
        public AppAuthBeginRequest(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public override async Task OnRequest()
        {
            //Get args
            if (!e.Request.Query.ContainsKey("nonce") || !e.Request.Query.ContainsKey("next") || !e.Request.Query.ContainsKey("type"))
            {
                await WriteString("Missing Required Args (client out of date?)", "text/plain", 400);
                return;
            }
            if (!int.TryParse(e.Request.Query["nonce"], out int nonce))
            {
                await WriteString("Invalid Nonce", "text/plain", 400);
                return;
            }
            if (!int.TryParse(e.Request.Query["type"], out int type))
            {
                await WriteString("Invalid Type", "text/plain", 400);
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
                preflight_token = session,
                custom_data = new Dictionary<string, string>
                {
                    {"BETA_KEY", e.Request.Query["beta_key"] }
                }
            };
            await Program.connection.system_preflight_tokens.InsertOneAsync(t);

            //Redirect to Steam auth
            string url = SteamAuth.SteamOpenID.Begin(session);
            e.Response.Headers.Add("Location", url);
            await WriteString("Redirecting to STEAM authentication.", "text/plain", 302);
        }
    }
}
