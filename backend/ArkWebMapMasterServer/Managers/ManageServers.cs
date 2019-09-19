using ArkWebMapMasterServer.PresistEntities;
using ArkWebMapMasterServer.PresistEntities.Managers;
using ArkWebMapMasterServer.Servers;
using LibDeltaSystem.Db.System;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArkWebMapMasterServer.Managers
{
    public static class ManageServers
    {
        public static LiteCollection<ArkManagerServer> GetServersCollection()
        {
            return Program.db.GetCollection<ArkManagerServer>("manager_servers");
        }

        public static ArkManagerServer BaseConstructServer(ArkManager m, ArkManagerMachine machine, string name, string clientId)
        {
            //Generate an id
            var collec = GetServersCollection();
            string id = Program.GenerateRandomString(24);
            while (collec.FindById(id) != null)
                id = Program.GenerateRandomString(24);

            //Create
            ArkManagerServer server = new ArkManagerServer
            {
                manager_id = m._id,
                name = name,
                machine_id = machine._id,
                client_id = clientId,
                _id = id,
                time = DateTime.UtcNow.Ticks
            };

            return server;
        }

        public static ArkManagerServer CreateArkServer(ArkManager provider, ArkManagerMachine machine, string name, string displayName, string clientId, ArkManagerServerGame_ARK settings)
        {
            //Construct the base server
            ArkManagerServer server = BaseConstructServer(provider, machine, name, clientId);
            server.game_settings = settings;

            //We'll now create an Ark server
            DbServer arkServer = new DbServer
            {
                display_name = displayName,
                _id = MongoDB.Bson.ObjectId.GenerateNewId(),
                image_url = DbServer.StaticGetPlaceholderIcon(name),
                owner_uid = clientId,
                server_creds = Program.GenerateRandomBytes(64),
                is_managed = true,
                provider_id = provider._id,
                provider_server_id = server._id,
                conn = Program.connection
            };
            server.linked_id = arkServer.id;

            //Insert
            Program.connection.system_servers.InsertOne(arkServer);
            GetServersCollection().Insert(server);

            return server;
        }

        public static void DeleteArkServer(ArkManagerServer mserver)
        {
            //Delete the linked Ark server
            Program.connection.GetServerByIdAsync(mserver.linked_id).GetAwaiter().GetResult().DeleteAsync().GetAwaiter().GetResult();

            //Delete the server
            GetServersCollection().Delete(mserver._id);
        }

        public static ArkManagerServer[] GetServers(ArkManager m)
        {
            return GetServersCollection().Find(x => x.manager_id == m._id).ToArray();
        }
    }
}
