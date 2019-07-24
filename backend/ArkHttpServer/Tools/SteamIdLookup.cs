using ArkBridgeSharedEntities.Entities;
using ArkBridgeSharedEntities.Requests;
using ArkHttpServer.NetEntities.TribeOverview;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkHttpServer.Tools
{
    public static class SteamIdLookup
    {
        private static List<CachedSteamIdInfo> cachedInfo = new List<CachedSteamIdInfo>();
        private const int EXPIRE_TIME_MINUTES = 30;

        public static Task<List<SteamProfile>> MassFetchPlayerData(List<string> ids)
        {
            List<SteamProfile> profiles = new List<SteamProfile>();
            List<string> idsToDownload = new List<string>();
            idsToDownload.AddRange(ids);

            //Find which profiles we already have cached
            DateTime maxTime = DateTime.UtcNow;
            var results = cachedInfo.Where(x => ids.Contains(x.id) && x.expire_time < maxTime);
            foreach(var r in results)
            {
                //Remove IDs that we do not need to check
                if (idsToDownload.Contains(r.id))
                {
                    idsToDownload.Remove(r.id);
                    profiles.Add(r.profile);
                }
            }
            
            //If we need any more IDs to be downloaded, use them
            if(idsToDownload.Count > 0)
            {
                //Create a payload to send
                MassFetchSteamDataPayload request = new MassFetchSteamDataPayload();
                request.ids = idsToDownload;

                //Request
                List<SteamProfile> downloadedProfiles = (List<SteamProfile>)MasterServerSender.SendRequestToMaster("mass_request_steam_info", request, typeof(List<SteamProfile>));
                profiles.AddRange(downloadedProfiles);

                //Add new profiles to the cache
                DateTime expireTime = DateTime.UtcNow + TimeSpan.FromMinutes(EXPIRE_TIME_MINUTES);
                foreach(var dp in downloadedProfiles)
                {
                    cachedInfo.Add(new CachedSteamIdInfo
                    {
                        expire_time = expireTime,
                        id = dp.steamid,
                        profile = dp
                    });
                }
            }

            //Respond with new 
            return Task.FromResult(profiles);
        }

        class CachedSteamIdInfo
        {
            public string id;
            public DateTime expire_time;
            public SteamProfile profile;
        }
    }
}
