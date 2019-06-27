using ArkBridgeSharedEntities.Entities.BasicTribeLog;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace ArkWebMapMasterServer.Tools
{
    public static class TribeHubTool
    {
        public static LiteCollection<BasicTribeLogEntry> GetCollection()
        {
            return Program.db.GetCollection<BasicTribeLogEntry>("tribe_log_hub");
        }

        public static BasicTribeLogEntry[] GetTribeLogEntries(List<Tuple<string, int>> serverTribeIds, int limit = int.MaxValue)
        {
            var collec = GetCollection();
            var results = collec.Find(x =>
               serverTribeIds.Where(y => y.Item1 == x.serverId && y.Item2 == x.tribeId).Count() == 1
                , 0, limit);
            return results.ToArray();
        }
    }
}
