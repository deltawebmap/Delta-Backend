using ArkBridgeSharedEntities.Entities;
using ArkBridgeSharedEntities.Entities.BasicTribeLog;
using ArkSaveEditor.World.WorldTypes;
using ArkSaveEditor.World.WorldTypes.ArkTribeLogEntries;
using ArkWebMapMasterServer.NetEntities;
using ArkWebMapMasterServer.PresistEntities;
using ArkWebMapMasterServer.Tools;
using CodeHollow.FeedReader;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Users
{
    public class HubService
    {
        public static Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, ArkUser u)
        {
            //Start download of ARK news in the background
            //Task<ArkHubWildcardNews> arkNewsDownloader = DownloadArkNews();

            //Load the UsersMe data.
            UsersMeReply usersMe = new UsersMeReply(u, true, false);

            //Grab tribe hub data
            List<Tuple<string, int>> serverTribeIds = new List<Tuple<string, int>>();
            foreach (var s in u.GetServers(true))
            {
                if(s.Item2 != null)
                    serverTribeIds.Add(new Tuple<string, int>(s.Item1._id, s.Item2.player_tribe_id));
            }
            BasicTribeLogEntry[] hubEntries = TribeHubTool.GetTribeLogEntries(serverTribeIds, 200);

            //Grab Steam profiles from hub data
            Dictionary<string, SteamProfile> steamProfiles = new Dictionary<string, SteamProfile>();
            foreach(var entr in hubEntries)
            {
                foreach(var sid in entr.steamIds)
                {
                    if (!steamProfiles.ContainsKey(sid))
                        steamProfiles.Add(sid, SteamUserRequest.GetSteamProfile(sid));
                }
            }

            //Await the Ark news to finish downloading.
            //ArkHubWildcardNews news = arkNewsDownloader.GetAwaiter().GetResult();

            //Produce the final output
            ArkHubReply reply = new ArkHubReply
            {
                //ark_news = news,
                servers = usersMe.servers,
                log = hubEntries,
                steam_profiles = steamProfiles
            };

            //Respond
            return Program.QuickWriteJsonToDoc(e, reply);
        }

        private static async Task<ArkHubWildcardNews> DownloadArkNews()
        {
            //Request item
            var feed = await FeedReader.ReadAsync("https://steamcommunity.com/games/346110/rss/");
            var item = feed.Items.First();

            //Extract the image and text from the article body by parsing the HTML
            var doc = new HtmlDocument();
            doc.LoadHtml(item.Description.Replace("<br>", "{NEWLINE}")); //Janky? Absolutely.

            //Format text
            var header = doc.DocumentNode.Descendants().Where(x => x.HasClass("bb_h1")).FirstOrDefault();
            if (header != null)
                header.Remove();
            string body = doc.DocumentNode.InnerText.Replace("{NEWLINE}", "\n").Trim('\n');

            //Extract other data
            string img = "https://steamcdn-a.akamaihd.net/steam/apps/346110/header.jpg";
            try
            {
                img = doc.DocumentNode.Descendants().Where(x => x.Name == "img").ToArray()[1].GetAttributeValue("src", "https://steamcdn-a.akamaihd.net/steam/apps/346110/header.jpg");
            } catch
            {
                //Ignore...
            }
            string title = item.Title;
            string link = item.Link;

            return new ArkHubWildcardNews
            {
                content = body,
                img = img,
                link = link,
                title = title
            };
        }
    }
}
