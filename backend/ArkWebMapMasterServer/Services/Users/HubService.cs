using ArkSaveEditor.World.WorldTypes;
using ArkSaveEditor.World.WorldTypes.ArkTribeLogEntries;
using ArkWebMapMasterServer.NetEntities;
using ArkWebMapMasterServer.PresistEntities;
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
            Task<ArkHubWildcardNews> arkNewsDownloader = DownloadArkNews();

            //Load the UsersMe data.
            UsersMeReply usersMe = new UsersMeReply(u, true, false);

            //Grab tribe hub data from offline data
            Dictionary<string, JToken> servers_hub = new Dictionary<string, JToken>();
            foreach (var s in u.GetServers())
            {
                ArkServer server = s.Item1;
                if(server.TryGetOfflineDataForTribe(u.steam_id, out DateTime time, out string offlineData))
                {
                    JObject jo = JObject.Parse(offlineData);
                    JToken hubData = jo["hub"];
                    servers_hub.Add(server._id, hubData);
                }
            }

            //Await the Ark news to finish downloading.
            ArkHubWildcardNews news = arkNewsDownloader.GetAwaiter().GetResult();

            //Produce the final output
            ArkHubReply reply = new ArkHubReply
            {
                ark_news = news,
                servers = usersMe.servers,
                servers_hub = servers_hub
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
