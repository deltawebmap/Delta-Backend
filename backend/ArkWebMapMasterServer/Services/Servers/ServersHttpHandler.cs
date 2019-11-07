using ArkWebMapMasterServer.NetEntities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using ArkBridgeSharedEntities.Entities;
using ArkWebMapMasterServer.Tools;
using LibDeltaSystem.Db.System;

namespace ArkWebMapMasterServer.Services.Servers
{
    public class ServersHttpHandler
    {
        public static async Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, string path)
        {
            //We're going to now get the server ID so we know what to use.
            string[] split = path.Split('/');
            string serverId = split[0];

            //Get the server by this ID
            DbServer server = await Program.connection.GetServerByIdAsync(serverId);

            //Handle server data
            if(split.Length > 1)
            {
                string nextUrl = path.Substring(serverId.Length + 1).TrimStart('/');

                //Authenticate the user
                DbUser user = Users.UsersHttpHandler.AuthenticateUser(e, true);

                //Look up the user's tribe by their steam ID
                int? tribeIdNullable = server.TryGetTribeIdAsync(user.steam_id).GetAwaiter().GetResult();
                bool hasTribe = tribeIdNullable.HasValue;
                if(!hasTribe)
                    throw new StandardError("You must be a part of this server to send API calls.", StandardErrorCode.NotPermitted);
                int tribeId = tribeIdNullable.Value;

                //Check if this is one of our URLs.
                if (nextUrl == "delete")
                    await DeleteServer.OnHttpRequest(e, server, user);
                else if (nextUrl == "edit")
                    await EditServerListing.OnHttpRequest(e, server);
                else if (nextUrl.StartsWith("mods"))
                    await ServerMods.OnHttpRequest(e, server);
                else if (nextUrl.StartsWith("put_user_prefs"))
                    await ServerUpdateSavedPrefs.OnUserPrefsRequest(e, server, user);
                else if (nextUrl.StartsWith("put_dino_prefs/"))
                    await ServerUpdateSavedPrefs.OnTribeDinoPrefsRequest(e, server, tribeId, nextUrl.Substring("put_dino_prefs/".Length));
                else
                    throw new StandardError("Not Found in Server", StandardErrorCode.NotFound);
            } else
            {
                //Return with some server info
                ArkServerReply se = new ArkServerReply(server, null);
                await Program.QuickWriteJsonToDoc(e, se);
            }
        }
    }
}
