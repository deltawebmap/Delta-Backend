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

        /// <summary>
        /// Creates a server and generates credentials
        /// </summary>
        /// <param name="name"></param>
        /// <param name="icon"></param>
        /// <returns></returns>
        public static DbServer CreateServer(string name, string icon, DbUser owner, bool isDemoServer = false)
        {
            //Get placeholder if needed
            if (icon == null)
                icon = DbServer.StaticGetPlaceholderIcon(name);

            //Create object
            DbServer server = new DbServer
            {
                display_name = name,
                _id = MongoDB.Bson.ObjectId.GenerateNewId(),
                image_url = icon,
                owner_uid = owner.id,
                server_creds = Program.GenerateRandomBytes(64),
                is_managed = false,
                is_published = false,
                has_custom_image = false,
                revision_id = 0,
                conn = Program.connection
            };

            //Insert
            Program.connection.system_servers.InsertOne(server);

            //Respond
            return server;
        } 
    }
}
