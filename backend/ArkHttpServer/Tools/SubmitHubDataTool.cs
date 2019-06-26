using ArkSaveEditor.World;
using ArkSaveEditor.World.WorldTypes.ArkTribeLogEntries;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkHttpServer.Tools
{
    public static class SubmitHubDataTool
    {
        public static void SubmitHubData(ArkWorld w)
        {

        }

        private static void FindNewLines(ArkWorld w)
        {
            //Find new, unsent, Ark hub lines
            List<ArkTribeLogEntry> events = new List<ArkTribeLogEntry>();
            List<string> searchSteamIds = new List<string>();
            foreach(var t in w.tribes)
            {
                w.tribes[0].GetTribeLog((string steamId) =>
                {
                    //Called when a Steam profile was found.
                    if (!searchSteamIds.Contains(steamId))
                        searchSteamIds.Add(steamId);
                });
            }
        }
    }
}
