using ArkBridgeSharedEntities.Entities;
using ArkWebMapMasterServer.PresistEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Users
{
    public static class UserDataRemover
    {
        public static async Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, ArkUser u, string token)
        {
            //Validate challenges
            if (Program.FindRequestMethod(e) != RequestHttpMethod.delete)
                throw new StandardError("A required challenge failed.", StandardErrorCode.MissingRequiredArg);
            if(e.Request.Query["chal_name"] != u.screen_name)
                throw new StandardError("A required challenge failed.", StandardErrorCode.MissingRequiredArg);

            //We're confirmed to delete this user. Go ahead and start removing data

            //We'll remove analytic data first
            using (HttpClient hc = new HttpClient())
                await hc.PostAsync("https://web-analytics.deltamap.net/v1/destroy?access_token=" + token, new StringContent(""));

            //Now, delete all servers we own
            var owned_servers = ArkWebMapMasterServer.Servers.ArkSlaveServerSetup.GetCollection().Find(x => x.owner_uid == u._id).ToArray();
            foreach (var s in owned_servers)
            {
                s.DeleteServer();
            }

            //Destroy all of our tokens
            var tokens = ArkWebMapMasterServer.Users.UserTokens.GetCollection().Find(x => x.uid == u._id).ToArray();
            foreach(var t in tokens)
            {
                ArkWebMapMasterServer.Users.UserTokens.GetCollection().Delete(t._id);
            }

            //Finally, destroy our user
            ArkWebMapMasterServer.Users.UserAuth.GetCollection().Delete(u._id);

            //Goodbye!
            await Program.QuickWriteStatusToDoc(e, true);
        }
    }
}
