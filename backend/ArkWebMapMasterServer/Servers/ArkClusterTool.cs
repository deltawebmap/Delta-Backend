using ArkWebMapMasterServer.PresistEntities;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapMasterServer.Servers
{
    public static class ArkClusterTool
    {
        public static LiteCollection<ArkCluster> GetCollection()
        {
            return Program.db.GetCollection<ArkCluster>("clusters");
        }

        public static ArkCluster GetClusterById(string id)
        {
            return GetCollection().FindById(id);
        }
    }
}
