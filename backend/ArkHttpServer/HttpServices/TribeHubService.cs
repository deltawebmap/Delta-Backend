using ArkBridgeSharedEntities.Entities;
using ArkBridgeSharedEntities.Requests;
using ArkSaveEditor.World;
using ArkSaveEditor.World.WorldTypes.ArkTribeLogEntries;
using ArkWebMapLightspeedClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkHttpServer.HttpServices
{
    public class TribeHubService
    {
        public static async Task OnHttpRequest(LightspeedRequest e, ArkWorld world, int tribeId)
        {
            //Generate reply
            await e.DoRespondJson(GenerateReply(world, tribeId));
            return;
        }

        public static TribeHubReply GenerateReply(ArkWorld world, int tribeId)
        {
            //Grab the tribe log for the requested player steam ID
            var searchTribes = world.tribes.Where(x => x.tribeId == tribeId).ToArray();
            List<ArkTribeLogEntry> events = new List<ArkTribeLogEntry>();
            List<string> searchSteamIds = new List<string>();
            if (searchTribes.Length != 1)
            {
                //No results.
                events = new List<ArkTribeLogEntry>();
            }
            else
            {
                events = searchTribes[0].GetTribeLog((string steamId) =>
                {
                    //Called when a Steam profile was found.
                    if (!searchSteamIds.Contains(steamId))
                        searchSteamIds.Add(steamId);
                });

            }

            //Lookup Steam IDs
            MassFetchSteamDataPayload request = new MassFetchSteamDataPayload();
            request.ids = searchSteamIds;
            List<SteamProfile> profiles = (List<SteamProfile>)MasterServerSender.SendRequestToMaster("mass_request_steam_info", request, typeof(List<SteamProfile>));

            //Convert this list into a dict
            Dictionary<string, SteamProfile> profilesDict = new Dictionary<string, SteamProfile>();
            foreach (SteamProfile prof in profiles)
            {
                if (!profilesDict.ContainsKey(prof.steamid))
                {
                    profilesDict.Add(prof.steamid, prof);
                }
            }

            //Send reply
            return new TribeHubReply
            {
                events = events,
                profiles = profilesDict,
                tribeid = tribeId
            };
        }

        public class TribeHubReply
        {
            public List<ArkTribeLogEntry> events;
            public Dictionary<string, SteamProfile> profiles;
            public int tribeid;
        }
    }
}
