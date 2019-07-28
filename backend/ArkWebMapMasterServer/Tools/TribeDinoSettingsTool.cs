using System;
using System.Collections.Generic;
using System.Text;
using ArkBridgeSharedEntities.Entities.Master;
using ArkWebMapMasterServer.PresistEntities;
using LiteDB;

namespace ArkWebMapMasterServer.Tools
{
    public static class TribeDinoSettingsTool
    {
        public static LiteCollection<DinoTribeSettings> GetCollection()
        {
            return Program.db.GetCollection<DinoTribeSettings>("dino_tribe_settings");
        }

        public static string CreateKey(ArkServer server, int tribeId, ulong dino)
        {
            return $"{server._id}/{tribeId.ToString()}/{dino.ToString()}";
        }
    }
}
