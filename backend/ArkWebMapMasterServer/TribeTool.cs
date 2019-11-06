using ArkBridgeSharedEntities.Entities;
using ArkWebMapMasterServer.PresistEntities;
using LibDeltaSystem.Db.Content;
using LibDeltaSystem.Db.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArkWebMapMasterServer
{
    public static class TribeTool
    {
        public static List<DbUser> GetTribePlayers(DbServer s, int tribeId)
        {
            //Get all
            return s.GetUsersByTribeAsync(tribeId).GetAwaiter().GetResult();
        }

        public static bool TryGetPlayerTribeId(DbServer s, DbUser u, out int tribeId)
        {
            tribeId = -1;

            //Grab the latest report and find all players with this tribe ID.
            DbPlayerProfile arkProfile = s.GetPlayerProfileBySteamIdAsync(u.steam_id).GetAwaiter().GetResult();

            //If the player was not found, fail
            if (arkProfile == null)
                return false;

            //Get the tribe ID of the first user.
            tribeId = arkProfile.tribe_id;
            return true;
        }
    }
}
