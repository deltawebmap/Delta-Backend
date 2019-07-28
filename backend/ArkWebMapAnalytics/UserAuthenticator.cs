using ArkWebMapAnalytics.NetEntities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace ArkWebMapAnalytics
{
    public static class UserAuthenticator
    {
        public static Dictionary<string, string> cached_uids = new Dictionary<string, string>();

        public static string GetUserIdFromToken(string token)
        {
            //Check if we have it cached
            if (cached_uids.ContainsKey(token))
                return cached_uids[token];

            //Fetch from server
            UsersMeReply user;
            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.Headers.Add("Authorization", "Bearer " + token);
                    string s = wc.DownloadString("https://deltamap.net/api/users/@me/");
                    user = JsonConvert.DeserializeObject<UsersMeReply>(s);
                }
            }
            catch (Exception ex)
            {
                return null;
            }

            //Add to cached uids
            if (!cached_uids.ContainsKey(token))
                cached_uids.Add(token, user.id);
            return user.id;
        }
    }
}
