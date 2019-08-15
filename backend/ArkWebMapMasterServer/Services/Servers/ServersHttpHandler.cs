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
using ArkWebMapMasterServer.Tools;
using ArkBridgeSharedEntities.Entities.BasicTribeLog;

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
                if(nextUrl == "hub")
                {
                    //Get hub data for this tribe
                    BasicTribeLogEntry[] hubEntries = TribeHubTool.GetTribeLogEntries(new List<Tuple<string, int>> { new Tuple<string, int>(server._id, tribeId) }, 200);

                    //Grab Steam profiles from hub data
                    Dictionary<string, SteamProfile> steamProfiles = new Dictionary<string, SteamProfile>();
                    foreach (var entr in hubEntries)
                    {
                        foreach (var sid in entr.steamIds)
                        {
                            if (!steamProfiles.ContainsKey(sid))
                                steamProfiles.Add(sid, SteamUserRequest.GetSteamProfile(sid));
                        }
                    }

                    //Write
                    return Program.QuickWriteJsonToDoc(e, new SingleServerHub
                    {
                        log = hubEntries,
                        profiles = steamProfiles
                    });
                }
                if (nextUrl.StartsWith("dinos/") && hasTribe)
                    return DinoSettingsHandler.OnHttpRequest(e, nextUrl.Substring(5), user, server, hasTribe, tribeId);

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
