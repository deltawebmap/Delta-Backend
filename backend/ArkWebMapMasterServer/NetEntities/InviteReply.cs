using ArkWebMapMasterServer.PresistEntities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapMasterServer.NetEntities
{
    public class InviteReply
    {
        public ArkServerReply server;
        public ArkServerInvite invite;

        public string inviter_name;
        public string inviter_user_icon;

        public InviteReply(ArkServerInvite i)
        {
            invite = i;
            ArkUser u = Users.UserAuth.GetUserById(i.inviter_uid);
            inviter_name = u.screen_name;
            inviter_user_icon = u.profile_image_url;
            server = new ArkServerReply(Servers.ArkSlaveServerSetup.GetSlaveServerById(i.server_uid), u);
        }
    }
}
