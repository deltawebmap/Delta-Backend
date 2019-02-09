using ArkWebMapMasterServer.NetEntities;
using ArkWebMapMasterServer.PresistEntities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Users
{
    public class UsersMe
    {
        public static Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, ArkUser u)
        {
            //Just convert it.
            return Program.QuickWriteJsonToDoc(e, new UsersMeReply(u));
        }
    }
}
