using ArkHttpServer.Entities;
using ArkHttpServer.NetEntities.TribeOverview;
using ArkSaveEditor.World.WorldTypes;
using System;
using System.Collections.Generic;
using System.Text;
using static ArkHttpServer.HttpServices.TribeHubService;

namespace ArkBridgeSharedEntities.Entities
{
    /// <summary>
    /// Report from the slave sent to the master server on startup.
    /// </summary>
    public class ArkSlaveReport
    {
        public string map_name;
        public float map_time;
        public DateTime lastSaveTime;
        public List<ArkSlaveReport_PlayerAccount> accounts;
    }

    public class ArkSlaveReport_PlayerAccount
    {
        public string player_steam_id { get; set; }
        public int player_tribe_id { get; set; }
        public bool allow_player { get; set; } //Can be used as a sort of ban list in the future.

        public string player_name { get; set; }
        public string player_tribe_name { get; set; }
    }

    public class ArkSlaveReport_OfflineTribe
    {
        public TribeOverviewReply overview { get; set; }
        public TribeHubReply hub { get; set; }
        public BasicTribe tribe { get; set; }
        public BasicArkWorld session { get; set; }
    }
}
