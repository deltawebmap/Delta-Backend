using ArkWebMapDynamicTiles.MapSessions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapDynamicTiles
{
    public static class SessionTool
    {
        private static ConcurrentDictionary<string, MapSession> sessions = new ConcurrentDictionary<string, MapSession>();

        public static MapSession GetSession(string token)
        {
            MapSession s = null;
            sessions.TryGetValue(token, out s);
            return s;
        }

        public static string AddSession(MapSession s)
        {
            string token = Program.GenerateRandomString(16);
            while (!sessions.TryAdd(token, s))
                token = Program.GenerateRandomString(16);
            return token;
        }

        public static int PurgeSessions()
        {
            //Find all old sessions and purge them
            List<string> purge = new List<string>();

            //Loop through all
            DateTime oldestTime = DateTime.UtcNow.AddMilliseconds(-(Program.HEARTBEAT_POLICY_MS + Program.HEARTBEAT_EXPIRE_TIME_ADD));
            foreach(var s in sessions)
            {
                if (s.Value.last_heartbeat < oldestTime)
                    purge.Add(s.Key);
            }

            //Remove
            foreach (var s in purge)
                sessions.TryRemove(s, out MapSession so);

            //Respond with count
            return purge.Count;
        }
    }
}
