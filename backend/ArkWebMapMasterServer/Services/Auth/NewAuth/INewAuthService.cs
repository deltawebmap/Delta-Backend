using LibDeltaSystem;
using LibDeltaSystem.Db.System;
using LibDeltaSystem.WebFramework;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Auth.NewAuth
{
    public abstract class INewAuthService : DeltaWebService
    {
        public INewAuthService(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public override async Task<bool> OnPreRequest()
        {
            return true;
        }

        public override async Task<bool> SetArgs(Dictionary<string, string> args)
        {
            return true;
        }

        public async Task<DbAuthenticationSession> GetAuthSessionAsync(string token)
        {
            var filterBuilder = Builders<DbAuthenticationSession>.Filter;
            var filter = filterBuilder.Eq("session_token", token);
            return await (await conn.system_auth_sessions.FindAsync(filter)).FirstOrDefaultAsync();
        }
    }
}
