using ArkBridgeSharedEntities.Entities;
using ArkWebMapMasterServer.NetEntities;
using ArkWebMapMasterServer.PresistEntities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Servers
{
    public static class EditServerListing
    {
        public static Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, ArkServer s)
        {
            //Open payload
            EditServerListingPayload payload = Program.DecodePostBody<EditServerListingPayload>(e);

            //Authenticate user
            ArkUser user = ArkWebMapMasterServer.Services.Users.UsersHttpHandler.AuthenticateUser(e, true);

            //Ensure user owns server
            if (user._id != s.owner_uid)
                throw new StandardError("You do not own this server.", StandardErrorCode.NotPermitted);

            //Update
            EditServer(s, payload);            

            //Apply to server
            s.Update();

            //Return OK
            return Program.QuickWriteStatusToDoc(e, true);
        }

        public static void EditServer(ArkServer s, EditServerListingPayload payload)
        {
            //Update name if sent
            if (payload.name != null)
            {
                //Validate name
                string name = payload.name;
                if (name.Length > 24 || name.Length < 2)
                    throw new StandardError("Please keep the name between 2-24 characters.", StandardErrorCode.InvalidInput);

                //Write
                s.display_name = name;
            }

            //Update icon if sent
            if (payload.iconToken != null)
            {
                //Fetch additional details.
                UserContentTokenPayload tokenPayload;
                using (WebClient wc = new WebClient())
                {
                    byte[] d = wc.DownloadData("https://user-content.romanport.com/upload_token?token=" + System.Web.HttpUtility.UrlEncode(payload.iconToken));
                    tokenPayload = JsonConvert.DeserializeObject<UserContentTokenPayload>(Encoding.UTF8.GetString(d));
                }

                //We've validated this image. Set it
                s.image_url = tokenPayload.url;
            }
        }
    }
}
