using ArkBridgeSharedEntities.Entities;
using ArkWebMapMasterServer.NetEntities;
using ArkWebMapMasterServer.PresistEntities;
using LibDeltaSystem.Db.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Users
{
    public static class TokenDevalidateService
    {
        private static void EnsureMethod(Microsoft.AspNetCore.Http.HttpContext e, DbUser u)
        {
            if (u == null)
                throw new StandardError("You must be signed in.", StandardErrorCode.AuthRequired);
            if (Program.FindRequestMethod(e) != RequestHttpMethod.post)
                throw new StandardError("Request must be POST or post.", StandardErrorCode.NotPermitted);
        }

        public static Task OnSingleDevalidate(Microsoft.AspNetCore.Http.HttpContext e, DbUser u, string token)
        {
            EnsureMethod(e, u);
            var tokenData = Program.connection.GetTokenByTokenAsync(token).GetAwaiter().GetResult();
            if(tokenData != null)
            {
                tokenData.DeleteAsync().GetAwaiter().GetResult();
                return Program.QuickWriteJsonToDoc(e, new OkReply
                {
                    ok = true
                });
            } else
            {
                throw new StandardError("Failed to find this token.", StandardErrorCode.MissingData);
            }
        }

        public static Task OnAllDevalidate(Microsoft.AspNetCore.Http.HttpContext e, DbUser u)
        {
            EnsureMethod(e, u);

            //Devalidate ALL tokens for this user
            u.DevalidateAllTokens().GetAwaiter().GetResult();

            //Return OK
            return Program.QuickWriteJsonToDoc(e, new OkReply
            {
                ok = true
            });
        }
    }
}
