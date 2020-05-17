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
    /// <summary>
    /// Used to query an app ID to just respond with data
    /// </summary>
    public class OAuthQuery : UserAuthDeltaService
    {
        public OAuthQuery(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

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

            //Get all scopes that are usable
            ulong scopeWrapped = ulong.Parse(request.scopes);
            List<OAuthScopeEntry> scopes = OAuthScopeStatics.GetOAuthScopes(scopeWrapped);
            if (scopeWrapped == 0)
            {
                await WriteString("No scopes provided.", "text/plain", 400);
                return;
            }

            //Determine if this is dangerous
            bool is_dangerous = false;
            foreach (var s in scopes)
                is_dangerous = is_dangerous || s.is_dangerous;

            //Create scopes URL
            string scopesSeparated = "";
            foreach (var s in scopes)
                scopesSeparated += s.id + ",";
            scopesSeparated.TrimEnd(',');

            //Respond
            string baseUrl = Program.connection.config.hosts.master + "/api";
            await WriteJSON(new OAuthInfoResponse
            {
                name = app.name,
                description = app.description,
                icon = app.icon_url,
                is_dangerous = is_dangerous,
                scopes = scopes,
                client_id = app.client_id
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

        class OAuthInfoResponse
        {
            public string name;
            public string description;
            public string icon;
            public List<OAuthScopeEntry> scopes;
            public bool is_dangerous;
            public string client_id;
        }
    }
}
