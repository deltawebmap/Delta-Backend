using ArkBridgeSharedEntities.Entities;
using ArkSaveEditor.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArkHttpServer.Init.WorldReport
{
    public static class WorldReportBuilder
    {
        public static bool SendWorldReport()
        {
            //Log
            Console.WriteLine("Sending world report...");
            
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
                if (tribeResults.Count() >= 1)
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

            //Now, send the report
            TrueFalseReply report_reply;
            try
            {
                report_reply = MasterServerSender.SendRequestToMaster<TrueFalseReply>("world_report", report);
            }
            catch (Exception ex)
            {
                return false;
            }
            return report_reply.ok;
        }
        
    }
}
