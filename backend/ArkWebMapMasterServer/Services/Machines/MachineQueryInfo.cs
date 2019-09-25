using LibDeltaSystem.Db.System;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Machines
{
    public static class MachineQueryInfo
    {
        /// <summary>
        /// Returns machine servers and other info.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="machine"></param>
        /// <returns></returns>
        public static async Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, DbMachine machine)
        {
            //Find all servers this machine uses
            List<DbServer> servers;
            {
                var filterBuilder = Builders<DbServer>.Filter;
                var filter = filterBuilder.Eq("machine_uid", machine.id);
                var response = await Program.connection.system_servers.FindAsync(filter);
                servers = await response.ToListAsync();
            }

            //Convert them
            List<MachineQueryInfoResponseServer> response_servers = new List<MachineQueryInfoResponseServer>();
            foreach(var s in servers)
            {
                response_servers.Add(new MachineQueryInfoResponseServer
                {
                    icon = s.image_url,
                    id = s.id,
                    load_settings = s.load_settings,
                    name = s.display_name,
                    token = s.token
                });
            }

            //Create and write response
            MachineQueryInfoResponse response_data = new MachineQueryInfoResponse
            {
                id = machine.id,
                name = machine.name,
                token = machine.token,
                servers = response_servers
            };
            await Program.QuickWriteJsonToDoc(e, response_data);
        }

        class MachineQueryInfoResponse
        {
            public string id;
            public string name;
            public string token;
            public List<MachineQueryInfoResponseServer> servers;
        }

        class MachineQueryInfoResponseServer
        {
            public string id;
            public string name;
            public string token;
            public string icon;
            public DbServer_LoadSettings load_settings;
        }
    }
}
