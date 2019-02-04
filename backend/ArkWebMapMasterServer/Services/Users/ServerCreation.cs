using ArkWebMapMasterServer.NetEntities;
using ArkWebMapMasterServer.PresistEntities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Users
{
    public static class ServerCreation
    {
        public static Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, ArkUser u)
        {
            //Require POST
            if (Program.FindRequestMethod(e) != RequestHttpMethod.post)
                throw new StandardError("Method can only be post.", StandardErrorCode.NotFound);

            //Read payload
            CreateServerPayload payload = Program.DecodePostBody<CreateServerPayload>(e);

            //Create the server
            ArkServer s = ArkWebMapMasterServer.Servers.ArkSlaveServerSetup.CreateServer(payload.name, "", u);

            //Make this user join the server
            u.servers.Add(s._id);
            u.Update();

            //Create an invite
            ArkServerInvite invite = ArkWebMapMasterServer.Servers.ArkServerInviteManager.CreateInvite(u, s, TimeSpan.FromDays(365 * 10));

            //Create reply
            CreateServerReply reply = new CreateServerReply
            {
                creds = Convert.ToBase64String(s.server_creds),
                id = s._id,
                ok = true,
                invite_url = invite.GetUrl()
            };
            return Program.QuickWriteJsonToDoc(e, reply);
        }
    }
}
