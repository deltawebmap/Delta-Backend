using ArkBridgeSharedEntities.Entities;
using ArkHttpServer.NetEntities;
using ArkWebMapMasterServer.PresistEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Users
{
    public static class TokenDevalidateService
    {
        private static void EnsureMethod(Microsoft.AspNetCore.Http.HttpContext e, ArkUser u)
        {
            if (u == null)
                throw new StandardError("You must be signed in.", StandardErrorCode.AuthRequired);
            if (Program.FindRequestMethod(e) != RequestHttpMethod.post)
                throw new StandardError("Request must be POST or post.", StandardErrorCode.NotPermitted);
        }

        public static Task OnSingleDevalidate(Microsoft.AspNetCore.Http.HttpContext e, ArkUser u, string token)
        {
            EnsureMethod(e, u);
            var collec = ArkWebMapMasterServer.Users.UserTokens.GetCollection();
            var foundTokens = collec.Find(x => x.token == token && x.uid == u._id);
            if(foundTokens.Count() == 1)
            {
                collec.Delete(foundTokens.First()._id);
                return Program.QuickWriteJsonToDoc(e, new OkReply
                {
                    ok = true
                });
            } else
            {
                throw new StandardError("Failed to find this token.", StandardErrorCode.MissingData);
            }
        }

        public static Task OnAllDevalidate(Microsoft.AspNetCore.Http.HttpContext e, ArkUser u)
        {
            EnsureMethod(e, u);
            //Devalidate ALL tokens for this user
            var collec = ArkWebMapMasterServer.Users.UserTokens.GetCollection();
            var foundTokens = collec.Find(x => x.uid == u._id);

            //Delete
            foreach(var t in foundTokens)
                collec.Delete(t._id);

            //Return OK
            return Program.QuickWriteJsonToDoc(e, new OkReply
            {
                ok = true
            });
        }
    }
}
