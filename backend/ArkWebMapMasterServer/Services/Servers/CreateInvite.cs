using ArkWebMapMasterServer.PresistEntities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Servers
{
    public class CreateInvite
    {
        public static Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, ArkServer s)
        {
            //For now, just authenticate us and create the invite
            return Program.QuickWriteJsonToDoc(e, ArkWebMapMasterServer.Servers.ArkServerInviteManager.CreateInvite(Users.UsersHttpHandler.AuthenticateUser(e, true), s, new TimeSpan(365 * 10, 0, 0, 0, 0)));
        }
    }
}
