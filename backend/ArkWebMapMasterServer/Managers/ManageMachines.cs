using ArkWebMapMasterServer.PresistEntities.Managers;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArkWebMapMasterServer.Managers
{
    public static class ManageMachines
    {
        public static LiteCollection<ArkManagerMachine> GetMachinesCollection()
        {
            return Program.db.GetCollection<ArkManagerMachine>("manager_machines");
        }

        public static ArkManagerMachine CreateMachine(ArkManager m, string name, string location)
        {
            //Generate an id
            var collec = GetMachinesCollection();
            string id = Program.GenerateRandomString(24);
            while (collec.FindById(id) != null)
                id = Program.GenerateRandomString(24);

            //Create and insert
            ArkManagerMachine machine = new ArkManagerMachine
            {
                created = DateTime.UtcNow.Ticks,
                location = location,
                name = name,
                ownerId = m._id,
                _id = id
            };
            collec.Insert(machine);
            return machine;
        }

        public static ArkManagerMachine GetMachineById(ArkManager m, string id)
        {
            return GetMachinesCollection().FindOne(x => x.ownerId == m._id && x._id == id);
        }

        public static ArkManagerMachine[] GetMachines(ArkManager m)
        {
            return GetMachinesCollection().Find(x => x.ownerId == m._id).ToArray();
        }
    }
}
