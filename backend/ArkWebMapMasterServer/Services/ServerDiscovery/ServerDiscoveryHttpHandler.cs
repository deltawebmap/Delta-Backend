using ArkBridgeSharedEntities.Entities;
using ArkWebMapMasterServer.PresistEntities;
using LibDeltaSystem.Db.System;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.ServerDiscovery
{
    public static class ServerDiscoveryHttpHandler
    {
        public static Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, string path)
        {
            //While optional, try to authenticate user
            DbUser user = Users.UsersHttpHandler.AuthenticateUser(e, false);

            if (path == "search")
                return SearchEndpoint.OnHttpRequest(e, user);

            throw new StandardError("Not found.", StandardErrorCode.NotFound);
        }
    }
}
