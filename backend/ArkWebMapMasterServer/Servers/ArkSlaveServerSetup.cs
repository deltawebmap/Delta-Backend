using ArkWebMapMasterServer.PresistEntities;
using LibDeltaSystem.Db.System;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapMasterServer.Servers
{
    public static class ArkSlaveServerSetup
    {
        /// <summary>
        /// Grabs the slave server by id, without validating the connection.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static DbServer GetSlaveServerById(string id)
        {
            return Program.connection.GetServerByIdAsync(id).GetAwaiter().GetResult();
        }
    }
}
