using ArkWebMapMasterServer.NetEntities;
using ArkWebMapMasterServer.PresistEntities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace ArkWebMapMasterServer
{
    public static class ServerPublishingManager
    {
        public const string COLLECTION_NAME = "published_server_listings";

        public const string UPLOAD_APPID_ICON = "Pc2Pk44XevX6C42m6Xu3Ag6J";
        public const string UPLOAD_APPID_BANNER = "mx0jep6ynWLMdOtsyGX0hG4E";

        public static ArkPublishedServerListing GetPublishedServer(string id)
        {
            return Program.db.GetCollection<ArkPublishedServerListing>(COLLECTION_NAME).FindOne( x => x._id == id);
        }

        public static void SavePublishedServer(ArkPublishedServerListing s, ArkServer server)
        {
            Program.db.GetCollection<ArkPublishedServerListing>(COLLECTION_NAME).Update(s);
            if(server.is_published != s.is_published)
            {
                server.is_published = s.is_published;
                server.Update();
            }
        }

        public static ArkPublishedServerListing CreatePublishedServer(ArkServer server, PublishedServerEdit request, bool doPublishIfOk, out string publishingFailureReason)
        {
            //Create bare minimum
            ArkPublishedServerListing l = new ArkPublishedServerListing();
            l._id = server._id;
            l.settings = new ArkPublishedServerSettings();
            l.saved_total_players = 0;
            l.saved_active_players = 0;
            l.map_name = server.latest_server_map;
            l.is_published = false;
            l.is_ip_verified = false;
            l.ip_address = request.ip_address;
            l.icon_url = server.image_url;
            if (!server.has_custom_image)
                l.icon_url = null;
            l.icon_removal_token = null;
            l.flags = new ArkPublishedServerFlags();
            l.display_name = server.display_name;
            l.display_motd = "";
            l.banner_url = null;
            l.banner_removal_token = null;


            //Now, handle editing like normal
            StandardEditPublishedServer(server, ref l, request);

            //Publish if requested
            if (doPublishIfOk)
                l.is_published = ValidateForPublishing(l, out publishingFailureReason);
            else
                publishingFailureReason = null;

            //Save
            Program.db.GetCollection<ArkPublishedServerListing>(COLLECTION_NAME).Insert(l);

            return l;
        }

        //Usually called from an HTTP POST request
        public static void StandardEditPublishedServer(ArkServer s, ref ArkPublishedServerListing l, PublishedServerEdit request)
        {
            bool isServerDirty = false;
            if (request.display_name != null && request.display_name != s.display_name)
            {
                //Update name on both
                if(request.display_name.Length > 24 || request.display_name.Length < 2)
                {
                    throw new Exception("Name was too long or too short.");
                } else
                {
                    isServerDirty = true;
                    s.display_name = request.display_name;
                    l.display_name = request.display_name;
                }
            }
            if(request.display_motd != null)
            {
                l.display_motd = request.display_motd;
            }
            if(request.banner_token != null)
            {
                ReadImgRequest(request.banner_token, out string url, out string removal_token, out string app_id);
                if (app_id != UPLOAD_APPID_BANNER)
                    throw new Exception("Failed to set banner image: Application IDs did not match, this file was uploaded under a different application.");
                l.banner_removal_token = removal_token;
                l.banner_url = url;
            }
            if(request.icon_token != null)
            {
                ReadImgRequest(request.icon_token, out string url, out string removal_token, out string app_id);
                if (app_id != UPLOAD_APPID_ICON)
                    throw new Exception("Failed to set icon image: Application IDs did not match, this file was uploaded under a different application.");
                l.icon_removal_token = removal_token;
                l.icon_url = url;
                s.image_url = url;
                s.has_custom_image = true;
                isServerDirty = true;
            }
            if(request.ip_address != null)
            {
                l.ip_address = request.ip_address;
                l.is_ip_verified = false;
            }
            if(request.flags != null)
            {
                l.flags = request.flags;
            }
            if(request.settings != null)
            {
                l.settings = request.settings;
            }
            if(request.language != null)
            {
                l.language = request.language;
            }
            if(request.location != null)
            {
                l.location = (ArkPublishedServerLocation)request.location;
            }
            if(request.discord_code != null)
            {
                if(request.discord_code == "")
                {
                    //Clear
                    l.discord_code = null;
                    l.discord_invite = null;
                } else
                {
                    //Validate code
                    DiscordInvite invite;
                    try
                    {
                        using (WebClient wc = new WebClient())
                        {
                            byte[] d = wc.DownloadData("https://discordapp.com/api/invites/" + System.Web.HttpUtility.UrlEncode(request.discord_code) + "?with_counts=true");
                            invite = JsonConvert.DeserializeObject<DiscordInvite>(Encoding.UTF8.GetString(d));
                        }
                    }
                    catch
                    {
                        throw new Exception("Could not validate Discord code.");
                    }
                    l.discord_invite = invite;
                    l.discord_code = invite.code;
                }
            }

            //Also set user counts
            l.saved_total_players = s.latest_server_local_accounts.Count;
            l.saved_active_players = s.latest_server_local_accounts.Count; //TODO: Actually count active players

            //If server is dirty, save
            if (isServerDirty)
                s.Update();
        }

        public static bool ValidateForPublishing(ArkPublishedServerListing l, out string reason)
        {
            //Check if IP address is valid
            if(!IPAddress.TryParse(l.ip_address, out IPAddress ip))
            {
                reason = "Failed to parse IP address.";
                return false;
            }
            if(l.icon_url == null)
            {
                reason = "Icon missing.";
                return false;
            }
            if(l.banner_url == null)
            {
                reason = "Banner missing.";
                return false;
            }
            reason = null;
            return true;
        }

        //Validates icon requests
        private static bool ReadImgRequest(string token, out string url, out string removal_token, out string app_id)
        {
            //Fetch additional details.
            UserContentTokenPayload tokenPayload;
            using (WebClient wc = new WebClient())
            {
                byte[] d = wc.DownloadData("https://user-content.romanport.com/upload_token?token=" + System.Web.HttpUtility.UrlEncode(token));
                tokenPayload = JsonConvert.DeserializeObject<UserContentTokenPayload>(Encoding.UTF8.GetString(d));
            }

            //We've validated this image. Set it
            url = tokenPayload.url;
            removal_token = tokenPayload.deletionToken;
            app_id = tokenPayload.applicationId;

            return true;
        }
    }
}
