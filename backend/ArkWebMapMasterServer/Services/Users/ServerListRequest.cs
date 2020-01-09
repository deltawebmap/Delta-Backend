using LibDeltaSystem.Db.System;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Users
{
    public static class ServerListRequest
    {
        public static async Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, DbUser u)
        {
            //Get servers
            var servers = await Program.connection.GetServersByOwnerAsync(u.id);

            //Convert
            ServerListResponse response = new ServerListResponse
            {
                servers = new List<ServerListResponseServer>(),
                token = await u.GetServerCreationToken()
            };
            foreach (var s in servers)
            {
                //Get map
                string mapName = null;
                var mapData = await s.GetMapEntryAsync(Program.connection);
                if (mapData != null)
                    mapName = mapData.displayName;

                //Write
                response.servers.Add(new ServerListResponseServer
                {
                    icon = s.image_url,
                    id = s.id,
                    map = mapName,
                    name = s.display_name
                });
            }

            //Write
            await Program.QuickWriteJsonToDoc(e, response);
        }

        class ServerListResponse
        {
            public List<ServerListResponseServer> servers;
            public string token;
        }

        class ServerListResponseServer
        {
            public string name;
            public string icon;
            public string id;
            public string map;
        }
    }
}
