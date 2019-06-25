using ArkWebMapMasterServer.NetEntities;
using ArkWebMapMasterServer.PresistEntities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using ArkBridgeSharedEntities.Entities;

namespace ArkWebMapMasterServer.Services.Servers
{
    public class ServersHttpHandler
    {
        public static Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, string path)
        {
            //We're going to now get the server ID so we know what to use.
            string[] split = path.Split('/');
            string serverId = split[0];

            //Get the server by this ID
            ArkServer server = ArkWebMapMasterServer.Servers.ArkSlaveServerSetup.GetSlaveServerById(serverId);

            //If there is content after this, proxy to this server. Else, return server info.
            if(split.Length > 1)
            {
                string nextUrl = path.Substring(serverId.Length + 1).TrimStart('/');

                //Check if this is a path that requires no auth
                if (nextUrl == "users")
                {
                    //Send back tribe users
                    return Program.QuickWriteJsonToDoc(e, server.latest_server_local_accounts);
                }

                //Authenticate the user
                ArkUser user = Users.UsersHttpHandler.AuthenticateUser(e, true);
                if (user.GetServers().Where(x => x.Item1._id == server._id).Count() != 1 && server.require_auth_to_view)
                    throw new StandardError("You must be a part of this server to send API calls.", StandardErrorCode.NotPermitted);

                //Look up the user's tribe by their steam ID
                bool hasTribe = server.TryGetTribeId(user.steam_id, out int tribeId);

                //Check if this is one of our URLs.
                if (nextUrl == "delete")
                {
                    //Leave
                    return DeleteServer.OnHttpRequest(e, server, user);
                }
                if(nextUrl == "edit")
                {
                    //Rename
                    return EditServerListing.OnHttpRequest(e, server);
                }
                if(nextUrl == "publish")
                {
                    return ServerPublishing.OnHttpRequest(e, server);
                }
                if(nextUrl.StartsWith("maps"))
                {
                    if (!hasTribe)
                        throw new StandardError("Could not find player tribe.", StandardErrorCode.NotPermitted);
                    return ServerMaps.OnHttpRequest(e, server, tribeId, nextUrl.Substring("maps".Length));
                }
                if(nextUrl == "offline_data")
                {
                    //Send back their offline tribe data, if they're in a tribe
                    if (!hasTribe)
                        throw new StandardError("Could not find player tribe.", StandardErrorCode.NotPermitted);
                    if (server.latest_offline_data == null)
                        throw new StandardError("This server has never sent offline data.", StandardErrorCode.MissingData);
                    if (!server.latest_offline_data.ContainsKey(tribeId))
                        throw new StandardError("No offline data was found for this tribe.", StandardErrorCode.MissingData);
                    return Program.QuickWriteToDoc(e, server.latest_offline_data[tribeId], "application/json");
                }

                throw new StandardError("Not Found in Server", StandardErrorCode.NotFound);
            } else
            {
                //Return with some server info
                ArkServerReply se = new ArkServerReply(server, null);
                return Program.QuickWriteJsonToDoc(e, se);
            }
        }
    }
}
