using ArkBridgeSharedEntities.Entities;
using ArkBridgeSharedEntities.Entities.Master;
using ArkWebMapMasterServer.NetEntities;
using ArkWebMapMasterServer.PresistEntities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Servers
{
    public static class ServerPublishing
    {
        public static Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, ArkServer s)
        {
            //Open payload
            PublishedServerEdit payload = Program.DecodePostBody<PublishedServerEdit>(e);

            //Authenticate user
            ArkUser user = ArkWebMapMasterServer.Services.Users.UsersHttpHandler.AuthenticateUser(e, true);

            //Ensure user owns server
            if (user._id != s.owner_uid)
                throw new StandardError("You do not own this server.", StandardErrorCode.NotPermitted);

            //Determine method
            RequestHttpMethod method = Program.FindRequestMethod(e);
            ArkPublishedServerListing listing = ServerPublishingManager.GetPublishedServer(s._id);

            if (method == RequestHttpMethod.post)
            {
                //Create or edit listing
                string failureReason;
                if (listing == null)
                {
                    //Create
                    listing = ServerPublishingManager.CreatePublishedServer(s, payload, true, out failureReason);
                }
                else
                {
                    //Edit
                    ServerPublishingManager.StandardEditPublishedServer(s, ref listing, payload);

                    //Publish if requested
                    listing.is_published = ServerPublishingManager.ValidateForPublishing(listing, out failureReason);

                    //Save
                    ServerPublishingManager.SavePublishedServer(listing, s);
                }

                //Return status
                ServerPublishingReply reply = new ServerPublishingReply
                {
                    is_published = failureReason == null,
                    publishing_failure_reason = failureReason,
                    listing = listing
                };
                return Program.QuickWriteJsonToDoc(e, reply);
            } else if (method == RequestHttpMethod.delete)
            {
                //Unpublish
                if(listing != null)
                {
                    listing.is_published = false;

                    //Save
                    ServerPublishingManager.SavePublishedServer(listing, s);
                }
                return Program.QuickWriteJsonToDoc(e, listing);
            } else if(method == RequestHttpMethod.get)
            {
                return Program.QuickWriteJsonToDoc(e, listing);
            } else
            {
                throw new StandardError("Unsupported method. Try POST, DELETE, or GET.", StandardErrorCode.BadMethod);
            }
        }

        class ServerPublishingReply
        {
            public bool is_published;
            public string publishing_failure_reason;

            public ArkPublishedServerListing listing;
        }
    }
}
