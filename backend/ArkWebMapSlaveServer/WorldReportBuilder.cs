using ArkBridgeSharedEntities.Entities;
using ArkHttpServer;
using ArkHttpServer.Entities;
using ArkHttpServer.HttpServices;
using ArkSaveEditor.World;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace ArkWebMapSlaveServer
{
    public static class WorldReportBuilder
    {
        public static ArkSlaveReport GenerateTribeOverview()
        {
            //Get world
            ArkWorld w = WorldLoader.GetWorld(out DateTime time);

            //Generate main data
            ArkSlaveReport report = new ArkSlaveReport();
            report.lastSaveTime = time;
            report.accounts = new System.Collections.Generic.List<ArkSlaveReport_PlayerAccount>();
            foreach (var player in w.players)
            {
                string tribeName = "";
                var tribeResults = w.tribes.Where(x => x.tribeId == player.tribeId);
                if(tribeResults.Count() >= 1)
                {
                    tribeName = tribeResults.First().tribeName;
                }

                report.accounts.Add(new ArkSlaveReport_PlayerAccount
                {
                    player_name = player.playerName,
                    allow_player = true,
                    player_steam_id = player.steamPlayerId,
                    player_tribe_id = player.tribeId,
                    player_tribe_name = tribeName
                });
            }
            report.map_name = w.map;
            report.map_time = w.gameTime;

            //Generate offline tribes
            report.offline_tribes = new Dictionary<int, string>();
            foreach(var t in w.tribes)
            {
                //Generate offline tribe
                ArkSlaveReport_OfflineTribe ot = GenerateOverviewForSingleTribe(w, t.tribeId, time);
                report.offline_tribes.Add(t.tribeId, JsonConvert.SerializeObject(ot));
            }

            //Return report
            return report;
        }

        static ArkSlaveReport_OfflineTribe GenerateOverviewForSingleTribe(ArkWorld w, int tribeId, DateTime lastSavedAtTime)
        {
            ArkSlaveReport_OfflineTribe result = new ArkSlaveReport_OfflineTribe();
            result.hub = TribeHubService.GenerateReply(w, tribeId);
            result.overview = TribeOverviewService.GenerateReply(w, tribeId);
            result.tribe = new BasicTribe(w, tribeId);
            result.session = new BasicArkWorld(w, lastSavedAtTime);
            return result;
        }
    }
}
