using ArkBridgeSharedEntities.Entities;
using ArkWebMapMasterServer.Managers;
using ArkWebMapMasterServer.NetEntities.Managers;
using ArkWebMapMasterServer.PresistEntities;
using ArkWebMapMasterServer.PresistEntities.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ArkWebMapMasterServer.Services.Providers.InternalApi
{
    public static class MachineConfigServiceHandler
    {
        public static Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, ArkManager user, ArkManagerMachine machine)
        {
            //Find all parent servers
            ArkManagerServer[] servers = ManageServers.GetServersCollection().Find(x => x.machine_id == machine._id && x.manager_id == user._id).ToArray();

            //Grab linked servers and their creds
            Dictionary<string, InternalMachineConfigResponseServerInfo> linked_servers = new Dictionary<string, InternalMachineConfigResponseServerInfo>();
            foreach(var s in servers)
            {
                ArkServer ark = ArkWebMapMasterServer.Servers.ArkSlaveServerSetup.GetSlaveServerById(s.linked_id);
                linked_servers.Add(s.linked_id, new InternalMachineConfigResponseServerInfo
                {
                    creds = ark.server_creds,
                    id = ark._id
                });
            }

            //Create and write response
            return Program.QuickWriteJsonToDoc(e, new InternalMachineConfigResponse
            {
                id = user._id,
                profile = user.profile,
                servers = servers,
                machine = machine,
                linked_servers = linked_servers
            });
        }
    }
}
