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

        public static Task OnAcceptInviteRequest(Microsoft.AspNetCore.Http.HttpContext e, ArkUser u)
        {
            //Get invite from URL
            string inviteId = e.Request.Query["id"];
            ArkServerInvite invite = ArkWebMapMasterServer.Servers.ArkServerInviteManager.GetInviteById(inviteId);
            if (invite == null)
                throw new StandardError("Invite not found", StandardErrorCode.NotFound);
            ArkWebMapMasterServer.Servers.ArkServerInviteManager.AcceptInvite(u, invite._id);

            //Return ok
            return Program.QuickWriteJsonToDoc(e, new OkReply
            {
                ok = true,
                message = "Invite opened."
            });
        }
    }
}
