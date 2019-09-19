using ArkBridgeSharedEntities.Entities;
using ArkWebMapMasterServer.PresistEntities;
using LibDeltaSystem.Db.System;
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
        public static async Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, DbUser u, string token)
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
            var owned_servers = u.GetOwnedServersAsync().GetAwaiter().GetResult();
            foreach (var s in owned_servers)
            {
                s.DeleteAsync().GetAwaiter().GetResult();
            }

            //Destroy all of our tokens
            u.DevalidateAllTokens().GetAwaiter().GetResult();

            //Finally, destroy our user
            u.DeleteAsync().GetAwaiter().GetResult();

            //Goodbye!
            await Program.QuickWriteStatusToDoc(e, true);
        }
    }
}
