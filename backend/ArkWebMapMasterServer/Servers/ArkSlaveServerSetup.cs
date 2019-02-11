using ArkWebMapMasterServer.PresistEntities;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapMasterServer.Servers
{
    public static class ArkSlaveServerSetup
    {
        public static LiteCollection<ArkServer> GetCollection()
        {
            return Program.db.GetCollection<ArkServer>("servers");
        }

        /// <summary>
        /// Grabs the slave server by id, without validating the connection.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static ArkServer GetSlaveServerById(string id)
        {
            var collec = GetCollection();
            var s = collec.FindOne(x => x._id == id);
            if (s.image_url == null)
                s.image_url = s.GetPlaceholderIcon();
            return s;
        }

        /// <summary>
        /// Creates a server and generates credentials
        /// </summary>
        /// <param name="name"></param>
        /// <param name="icon"></param>
        /// <returns></returns>
        public static ArkServer CreateServer(string name, string icon, ArkUser owner, bool isDemoServer = false)
        {
            //Generate a unique index
            var collec = GetCollection();
            string id = Program.GenerateRandomString(24);
            while (collec.Count(x => x._id == id) != 0)
                id = Program.GenerateRandomString(24);

            //Get placeholder if needed
            if (icon == null)
                icon = ArkServer.StaticGetPlaceholderIcon(name);

            //Create object
            ArkServer server = new ArkServer
            {
                display_name = name,
                _id = id,
                image_url = icon,
                owner_uid = owner._id,
                server_creds = Program.GenerateRandomBytes(64),
                require_auth_to_view = !isDemoServer,
                is_demo_server = isDemoServer,
                is_deleted = false
            };

            //Insert
            collec.Insert(server);

            //Respond
            return server;
        } 
    }
}
