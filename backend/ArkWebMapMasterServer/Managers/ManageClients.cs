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
    public static class ManageClients
    {
        public static LiteCollection<ArkManagerClient> GetClientsCollection()
        {
            return Program.db.GetCollection<ArkManagerClient>("manager_clients");
        }

        public static ArkManagerClient CreateClient(ArkManager m, string name)
        {
            //Creates a client.
            var collec = GetClientsCollection();
            
            //Generate a random ID
            string id = Program.GenerateRandomString(24);
            while (collec.FindOne(x => x._id == id) != null)
                id = Program.GenerateRandomString(24);

            //Generate a random invite code
            string inviteCode = Program.GenerateRandomString(8);
            while (collec.FindOne(x => x.invite_code == inviteCode) != null)
                inviteCode = Program.GenerateRandomString(24);

            //Create
            ArkManagerClient c = new ArkManagerClient
            {
                invite_code = inviteCode,
                linked_userid = null,
                manager_id = m._id,
                name = name,
                _id = id
            };
            collec.Insert(c);

            return c;
        }

        public static ArkManagerClient[] GetClients(ArkManager m)
        {
            return GetClientsCollection().Find(x => x.manager_id == m._id).ToArray();
        }
    }
}
