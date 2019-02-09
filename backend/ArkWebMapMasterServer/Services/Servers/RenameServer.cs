using ArkWebMapMasterServer.NetEntities;
using ArkWebMapMasterServer.PresistEntities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Servers
{
    public class RenameServer
    {
        public static Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, ArkServer s)
        {
            //Decode
            RenameServerRequest request = Program.DecodePostBody<RenameServerRequest>(e);

            //Authenticate user
            ArkUser user = ArkWebMapMasterServer.Services.Users.UsersHttpHandler.AuthenticateUser(e, true);

            //Ensure user owns server
            if (user._id != s.owner_uid)
                throw new StandardError("You do not own this server.", StandardErrorCode.NotPermitted);

            //Validate name
            if (request.name.Length > 24 || request.name.Length < 2)
                throw new StandardError("Please keep the name between 2-24 characters.", StandardErrorCode.InvalidInput);

            //Set name and save
            s.display_name = request.name;
            s.image_url = s.GetPlaceholderIcon();
            s.Update();

            //Return OK
            return Program.QuickWriteJsonToDoc(e, new ArkBridgeSharedEntities.Entities.TrueFalseReply
            {
                ok = true
            });
        }
    }
}
