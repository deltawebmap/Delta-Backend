using ArkBridgeSharedEntities.Entities;
using ArkBridgeSharedEntities.Entities.BasicTribeLog;
using ArkHttpServer.PersistEntities;
using ArkSaveEditor.World;
using ArkSaveEditor.World.WorldTypes.ArkTribeLogEntries;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkHttpServer.Tools
{
    public static class SubmitHubDataTool
    {
        public static void SubmitHubData(ArkWorld w, DateTime loadTime)
        {
            //Get new lines that have not been sent
            List<ArkTribeLogEntry> newEntries = FindNewLines(w, out List<string> steamIdsToInclude, out List <HistoricalTribeLogEntry> entriesToInsert);

            //Convert all
            List<BasicTribeLogEntry> convertedEntries = new List<BasicTribeLogEntry>();
            foreach (var e in newEntries)
                convertedEntries.Add(ConvertToBasicEntry(e, loadTime));

            //Submit
            BasicTribeLogSubmission submission = new BasicTribeLogSubmission
            {
                entries = convertedEntries,
                includeSteamIds = steamIdsToInclude
            };
            ArkWebServer.sendRequestToMasterCode("report_hub", submission, typeof(TrueFalseReply));

            //Sent. Now, apply the latest tribe log entries
            var collec = ArkWebServer.db.GetCollection<HistoricalTribeLogEntry>("latest_tribe_log_tribes");
            foreach(var t in entriesToInsert)
            {
                if (collec.FindById(t._id) != null)
                    collec.Update(t);
                else
                    collec.Insert(t);
            }
        }

        private static List<ArkTribeLogEntry> FindNewLines(ArkWorld w, out List<string> steamIdsToInclude, out List<HistoricalTribeLogEntry> entriesToInsert)
        {
            //Find new, unsent, Ark hub lines
            List<ArkTribeLogEntry> currentEvents = new List<ArkTribeLogEntry>();
            List<string> searchSteamIds = new List<string>();
            var collec = ArkWebServer.db.GetCollection<HistoricalTribeLogEntry>("latest_tribe_log_tribes");
            entriesToInsert = new List<HistoricalTribeLogEntry>();
            foreach (var t in w.tribes)
            {
                //Obtain tribe log
                var tribeEvents = w.tribes[0].GetTribeLog((string steamId) =>
                {
                    //Called when a Steam profile was found.
                    if (!searchSteamIds.Contains(steamId))
                        searchSteamIds.Add(steamId);
                });

                //Get the latest known entry for this tribe
                var latestKnownEntry = collec.FindOne(x => x._id == t.tribeId);
                if(latestKnownEntry == null)
                {
                    //Add all
                    currentEvents.AddRange(tribeEvents);
                } else
                {
                    //Add all of the events after this. First, find the index of the latest. Newest events start at the beginning of the array
                    for(int i = 0; i<tribeEvents.Count; i++)
                    {
                        if (tribeEvents[i].raw == latestKnownEntry.content)
                            break;

                        //Add
                        currentEvents.Add(tribeEvents[i]);
                    }
                }

                //Now, update the latest if this entire process completes without errors
                if(tribeEvents.Count > 0)
                {
                    entriesToInsert.Add(new HistoricalTribeLogEntry
                    {
                        content = tribeEvents[0].raw,
                        time = DateTime.UtcNow.Ticks,
                        _id = t.tribeId
                    });
                }
            }

            steamIdsToInclude = searchSteamIds;
            return currentEvents;
        }

        private static BasicTribeLogEntry ConvertToBasicEntry(ArkTribeLogEntry b, DateTime time)
        {
            //Create base
            List<string> steamIds = new List<string>();
            BasicTribeLogEntry e = new BasicTribeLogEntry
            {
                type = b.type,
                tribeId = b.tribeId,
                time = time,
                serverId = b.serverId,
                priority = b.priority,
                gameTime = b.gameTime,
                gameDay = b.gameDay,
                targets = new Dictionary<string, BasicTribeLogPlayerOrDinoTarget>()
            };

            if (b.type == ArkTribeLogEntryType.TargetKilledTarget)
            {
                ArkTribeLogEntry_TargetKilledTarget be = (ArkTribeLogEntry_TargetKilledTarget)b;
                e.targets.Add("killer", new BasicTribeLogPlayerOrDinoTarget(be.killer, ref steamIds));
                e.targets.Add("victim", new BasicTribeLogPlayerOrDinoTarget(be.victim, ref steamIds));
            }
            if (b.type == ArkTribeLogEntryType.TribeTamedDino)
            {
                ArkTribeLogEntry_Tamed be = (ArkTribeLogEntry_Tamed)b;
                e.targets.Add("tamedTarget", new BasicTribeLogPlayerOrDinoTarget(be.tamedTarget));
                e.targets.Add("tribePlayerTarget", new BasicTribeLogPlayerOrDinoTarget(be.tribePlayerTarget, ref steamIds));
            }

            e.steamIds = steamIds;
            return e;
        }
    }
}
