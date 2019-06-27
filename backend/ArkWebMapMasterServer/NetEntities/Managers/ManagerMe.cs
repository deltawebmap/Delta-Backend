using ArkWebMapMasterServer.Managers;
using ArkWebMapMasterServer.PresistEntities.Managers;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapMasterServer.NetEntities.Managers
{
    public class ManagerMe
    {
        public string id;
        public string name;
        public string icon;
        public ArkManagerMachine[] machines;
        public ArkManagerServer[] servers;

        public static ManagerMe Generate(ArkManager m)
        {
            ManagerMe o = new ManagerMe
            {
                icon = m.profile.wide_image_url,
                id = m._id,
                name = m.profile.name,
                machines = ManageMachines.GetMachines(m),
                servers = ManageServers.GetServers(m)
            };
            return o;
        }
    }
}
