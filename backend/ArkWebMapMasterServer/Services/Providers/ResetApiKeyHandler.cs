using ArkBridgeSharedEntities.Entities;
using ArkWebMapMasterServer.Managers;
using ArkWebMapMasterServer.NetEntities.Managers;
using ArkWebMapMasterServer.PresistEntities.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Providers
{
    public static class ResetApiKeyHandler
    {
        public static Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, ArkManager u)
        {
            //Generate an API token
            var collec = Managers.ManageAuth.GetManagersCollection();
            string apiToken = Program.GenerateRandomString(64);
            while (collec.Find(x => x.api_token == apiToken).Count() != 0)
                apiToken = Program.GenerateRandomString(64);

            //Set and save
            u.api_token = apiToken;
            collec.Update(u);

            //Write
            return Program.QuickWriteJsonToDoc(e, new UpdateApiTokenResponse
            {
                api_token = apiToken
            });
        }
    }
}
