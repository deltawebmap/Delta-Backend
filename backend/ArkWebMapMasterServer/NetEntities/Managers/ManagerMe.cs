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
        public string api_token;
        public ArkManagerMachine[] machines;
        public ArkManagerServer[] servers;
        public ArkManagerClient[] clients;

        public static ManagerMe Generate(ArkManager m)
        {
            ManagerMe o = new ManagerMe
            {
                icon = m.profile.wide_image_url,
                id = m._id,
                name = m.profile.name,
                api_token = m.api_token,
                machines = ManageMachines.GetMachines(m),
                servers = ManageServers.GetServers(m),
                clients = ManageClients.GetClients(m)
            };
            return o;
        }
    }
}
