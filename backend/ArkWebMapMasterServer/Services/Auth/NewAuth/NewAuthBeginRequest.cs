using LibDeltaSystem;
using LibDeltaSystem.Db.System;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Auth.NewAuth
{
    public class NewAuthBeginRequest : INewAuthService
    {
        public NewAuthBeginRequest(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public const int SCOPE_INDEX_COUNT = 3;

        public override async Task OnRequest()
        {
            //Parse request data
            RequestData r = await ReadPOSTContentChecked<RequestData>();
            if (r == null)
                return;

            //Get the oauth application
            var app = await conn.GetOAuthAppByAppID(r.application_id);
            if(app == null)
            {
                await WriteErrorResponse("Application was not found.");
                return;
            }

            //Make scope
            uint scope;
            if (!app.is_official)
            {
                //Pull scope from params
                if (!uint.TryParse(r.scope, out scope))
                {
                    await WriteErrorResponse("Scope is missing. You may have followed a broken link.");
                    return;
                }

                //Check if any invalid flags were set
                for (int i = SCOPE_INDEX_COUNT; i < 31; i++)
                {
                    if (((scope >> i) & 1U) == 1)
                    {
                        await WriteErrorResponse("Scope is illegal. Please request fewer permissions.");
                        return;
                    }
                }

                //Make sure we've requested *a* scope
                if(scope == 0)
                {
                    await WriteErrorResponse("No scopes were requested.");
                    return;
                }
            } else
            {
                //Set scope to everything if this is an official app
                scope = uint.MaxValue;
            }  

            //Get oauth owner
            var appOwner = await conn.GetUserByIdAsync(app.owner_id);

            //Create session token
            string token = LibDeltaSystem.Tools.SecureStringTool.GenerateSecureString(48);
            while(await GetAuthSessionAsync(token) != null)
                token = LibDeltaSystem.Tools.SecureStringTool.GenerateSecureString(48);

            //Make session
            var s = new DbAuthenticationSession
            {
                _id = ObjectId.GenerateNewId(),
                application_id = app._id,
                custom_data = new Dictionary<string, string>
                {
                    {CUSTOM_DATA_KEY__OAUTH_CUSTOM_DATA, r.custom }
                },
                nonce = r.nonce,
                session_token = token,
                state = DbAuthenticationSession.AuthState.PendingExternalAuth,
                scope = scope
            };
            await conn.system_auth_sessions.InsertOneAsync(s);

            //Create return URL
            string return_url = $"{Program.connection.config.hosts.master}/api/auth/authenticate?token={s.session_token}&nonce={r.nonce}";
            string encoded_return_url = System.Web.HttpUtility.UrlEncode(return_url);
            string url = $"https://steamcommunity.com/openid/login?openid.return_to={encoded_return_url}&openid.mode=checkid_setup&openid.ns=http%3A%2F%2Fspecs.openid.net%2Fauth%2F2.0&openid.identity=http%3A%2F%2Fspecs.openid.net%2Fauth%2F2.0%2Fidentifier_select&openid.claimed_id=http%3A%2F%2Fspecs.openid.net%2Fauth%2F2.0%2Fidentifier_select&openid.ns.sreg=http%3A%2F%2Fopenid.net%2Fextensions%2Fsreg%2F1.1&openid.realm={encoded_return_url}";

            //Create response
            await WriteJSON(new ResponseData
            {
                ok = true,
                fail_reason = null,
                session = new ResponseData_Session
                {
                    session_token = token,
                    next_url = url,
                    scope = scope,
                    app = new ResponseData_AppInfo
                    {
                        title = app.name,
                        description = app.description,
                        icon = app.icon_url,
                        is_official = app.is_official,
                        is_verified = app.is_verified,
                        author_id = appOwner.id,
                        author_icon = appOwner.profile_image_url,
                        author_name = appOwner.screen_name,
                        creation_date = app.creation_date
                    }
                }
            });
        }

        private async Task WriteErrorResponse(string reason)
        {
            await WriteJSON(new ResponseData
            {
                ok = false,
                fail_reason = reason
            });
        }

        public class RequestData
        {
            public string application_id;
            public string nonce;
            public string referrer;
            public string custom;
            public string scope;
        }

        public class ResponseData
        {
            public bool ok;
            public string fail_reason;
            public ResponseData_Session session;
        }

        public class ResponseData_Session
        {
            public string session_token;
            public ResponseData_AppInfo app;
            public string next_url;
            public uint scope;
        }

        public class ResponseData_AppInfo
        {
            public string title;
            public string description;
            public string icon;
            public bool is_official; //Will just say you're signing into Delta Web Map, not an application
            public bool is_verified;
            public string author_id;
            public string author_name;
            public string author_icon;
            public DateTime creation_date;
        }
    }
}
