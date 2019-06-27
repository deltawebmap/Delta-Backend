using ArkWebMapMasterServer.PresistEntities;
using ArkWebMapMasterServer.PresistEntities.Managers;
using ArkWebMapMasterServer.Servers;
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

            //Generate a unique index
            var arkCollec = ArkSlaveServerSetup.GetCollection();
            string id = Program.GenerateRandomString(24);
            while (arkCollec.Count(x => x._id == id) != 0)
                id = Program.GenerateRandomString(24);

            //We'll now create an Ark server
            ArkServer arkServer = new ArkServer
            {
                display_name = displayName,
                _id = id,
                image_url = ArkServer.StaticGetPlaceholderIcon(name),
                owner_uid = clientId,
                server_creds = Program.GenerateRandomBytes(64),
                require_auth_to_view = true,
                is_demo_server = false,
                is_deleted = false,
                is_managed = true,
                provider_id = provider._id,
                provider_server_id = server._id
            };
            server.linked_id = arkServer._id;

            //Insert
            arkCollec.Insert(arkServer);
            GetServersCollection().Insert(server);

            return server;
        }

        public static ArkManagerServer[] GetServers(ArkManager m)
        {
            return GetServersCollection().Find(x => x.manager_id == m._id).ToArray();
        }
    }
}
