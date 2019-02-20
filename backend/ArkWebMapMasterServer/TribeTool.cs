using ArkBridgeSharedEntities.Entities;
using ArkWebMapMasterServer.PresistEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArkWebMapMasterServer
{
    public static class TribeTool
    {
        public static List<ArkUser> GetTribePlayers(ArkServer s, int tribeId)
        {
            //If the server has never sent a report, cancel.
            if (!s.has_server_report)
                return new List<ArkUser>();

            //Grab the latest report and find all players with this tribe ID.
            ArkSlaveReport_PlayerAccount[] arkProfiles = s.latest_server_local_accounts.Where(x => x.player_tribe_id == tribeId).ToArray();

            //Find all user accounts that use these Steam IDs
            ArkUser[] users = Users.UserAuth.GetCollection().Find(x => arkProfiles.Where(y => y.player_steam_id == x.steam_id).Count() >= 1).ToArray();

            //Convert to list and return
            return users.ToList();
        }

        public static bool TryGetPlayerTribeId(ArkServer s, ArkUser u, out int tribeId)
        {
            tribeId = -1;
            //If the server has never sent a report, cancel.
            if (!s.has_server_report)
                return false;

            //Grab the latest report and find all players with this tribe ID.
            ArkSlaveReport_PlayerAccount[] arkProfiles = s.latest_server_local_accounts.Where(x => x.player_steam_id == u.steam_id).ToArray();

            //If the player was not found, fail
            if (arkProfiles.Length == 0)
                return false;

            //Get the tribe ID of the first user.
            tribeId = arkProfiles[0].player_tribe_id;
            return true;
        }
    }
}
