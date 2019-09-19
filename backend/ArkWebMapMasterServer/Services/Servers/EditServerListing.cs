using ArkBridgeSharedEntities.Entities;
using ArkWebMapMasterServer.NetEntities;
using ArkWebMapMasterServer.PresistEntities;
using ArkWebMapMasterServer.Servers;
using LibDeltaSystem.Db.System;
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
        public static Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, DbServer s)
        {
            //Open payload
            EditServerListingPayload payload = Program.DecodePostBody<EditServerListingPayload>(e);

            //Authenticate user
            DbUser user = ArkWebMapMasterServer.Services.Users.UsersHttpHandler.AuthenticateUser(e, true);

            //Ensure user owns server
            if (user.id != s.owner_uid)
                throw new StandardError("You do not own this server.", StandardErrorCode.NotPermitted);

            //Update
            EditServer(s, payload, user);            

            //Apply to server
            s.Update();

            //Return OK
            return Program.QuickWriteStatusToDoc(e, true);
        }

        public static void EditServer(DbServer s, EditServerListingPayload payload, DbUser user)
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
                s.has_custom_image = true;
            }

            //Update cluster ID
            if(payload.clusterId != null)
            {
                if(payload.clusterId == "")
                {
                    //Clear cluster
                    s.cluster_id = null;
                } else
                {
                    //Verify cluster
                    ArkCluster cluster = ArkClusterTool.GetClusterById(payload.clusterId);
                    if (cluster == null)
                        throw new StandardError("Cluster ID not found.", StandardErrorCode.InvalidInput);
                    if(cluster.owner_id != user.id)
                        throw new StandardError("You do not own this cluster ID.", StandardErrorCode.InvalidInput);
                    s.cluster_id = payload.clusterId;
                }
            }
        }
    }
}
