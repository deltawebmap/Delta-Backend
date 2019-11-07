using ArkWebMapMasterServer.NetEntities;
using LibDeltaSystem.Db.System;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Users
{
    public class UsersMe
    {
        public static Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, DbUser u)
        {
            //Check if we should hide invalid. By default, no
            bool hideInvalid = false;
            if(e.Request.Query.ContainsKey("hideInvalid"))
            {
                hideInvalid = e.Request.Query["hideInvalid"] == "true";
            }

            //Just convert it.
            UsersMeReply user = new UsersMeReply();
            user.MakeUsersMe(u, hideInvalid);
            return Program.QuickWriteJsonToDoc(e, user);
        }

        public static async Task OnMachineListRequest(Microsoft.AspNetCore.Http.HttpContext e, DbUser u)
        {
            //Search the database for machines owned by this user
            var filterBuilder = Builders<DbMachine>.Filter;
            var filter = filterBuilder.Eq("owner_type", "USER") & filterBuilder.Eq("owner_id", u.id);
            var results = await Program.connection.system_machines.FindAsync(filter);
            var machines = await results.ToListAsync();

            //Now, create a response with these
            List<ResponseMachine> response = new List<ResponseMachine>();
            foreach(var m in machines)
            {
                //Find how many servers use this machine
                m.conn = Program.connection;
                var servers = await m.GetServersAsync();

                //Convert servers
                List<ResponseServer> responseServers = new List<ResponseServer>();
                foreach(var s in servers)
                {
                    responseServers.Add(new ResponseServer
                    {
                        icon_url = s.image_url,
                        id = s.id,
                        name = s.display_name
                    });
                }

                //Add response
                response.Add(new ResponseMachine
                {
                    id = m.id,
                    name = m.name,
                    token = m.token,
                    servers = responseServers
                });
            }
            await Program.QuickWriteJsonToDoc(e, response);
        }

        class ResponseMachine
        {
            public string name;
            public string id;
            public string token;
            public List<ResponseServer> servers;
        }

        class ResponseServer
        {
            public string id;
            public string name;
            public string icon_url;
        }
    }
}
