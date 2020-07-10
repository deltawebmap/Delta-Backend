using LibDeltaSystem;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Auth.NewAuth
{
    public class NewAuthAppValidateRequest : INewAuthService
    {
        public NewAuthAppValidateRequest(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public override async Task OnRequest()
        {
            //Get request data
            var r = await ReadPOSTContentChecked<RequestData>();
            if (r == null)
                return;

            //Get session
            var session = await GetAuthSessionAsync(r.oauth_token);
            if(session == null)
            {
                await WriteString("Invalid session ID.", "text/plain", 400);
                return;
            }

            //Ensure session is in the right state
            if(session.state != LibDeltaSystem.Db.System.DbAuthenticationSession.AuthState.PendingOauthAuth)
            {
                await WriteString("Invalid session state!", "text/plain", 400);
                return;
            }

            //Get app
            var app = await conn.GetOAuthAppByInternalID(session.application_id);

            //Validate client IDs
            if(r.client_id != app.client_id)
            {
                await WriteString("Invalid client ID!", "text/plain", 400);
                return;
            }
            if (r.client_secret != app.client_secret)
            {
                await WriteString("Invalid client secret!", "text/plain", 400);
                return;
            }

            //Get the scope to use
            uint scope = (uint)session.scope;

            //Get user and generate the token
            ObjectId userId = ObjectId.Parse(session.custom_data[CUSTOM_DATA_KEY__USER_ID]);
            var user = await conn.GetUserByIdAsync(userId);
            var token = await user.MakeOauthToken(conn, app._id, scope);

            //Delete session
            await session.DeleteAsync(conn);

            //Create response data
            ResponseData output = new ResponseData
            {
                token = token,
                scope = scope,
                client_id = app.client_id,
                custom_data = session.custom_data[CUSTOM_DATA_KEY__OAUTH_CUSTOM_DATA],
                user = new ResponseData_User
                {
                    id = user.id,
                    platform_id = user.steam_id,
                    name = user.screen_name,
                    icon = user.profile_image_url
                }
            };
            await WriteJSON(output);
        }

        class RequestData
        {
            public string oauth_token;
            public string client_id;
            public string client_secret;
        }

        class ResponseData
        {
            public string token;
            public uint scope;
            public string client_id;
            public string custom_data;  
            public ResponseData_User user;
        }

        class ResponseData_User
        {
            public string id;
            public string platform_id;
            public string name;
            public string icon;
        }
    }
}
