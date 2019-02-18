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
            //Check if we should hide invalid. By default, no
            bool hideInvalid = false;
            if(e.Request.Query.ContainsKey("hideInvalid"))
            {
                hideInvalid = e.Request.Query["hideInvalid"] == "true";
            }

            //Check if the user wants us to ping all servers.
            bool pingServers = false;
            if (e.Request.Query.ContainsKey("pingAllServers"))
            {
                pingServers = e.Request.Query["pingAllServers"] == "true";
            }

            //Just convert it.
            return Program.QuickWriteJsonToDoc(e, new UsersMeReply(u, hideInvalid, pingServers));
        }
    }
}
