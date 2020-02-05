using ArkWebMapMasterServer.NetEntities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

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
            if (server == null)
                throw new StandardError("Server Not Found", StandardErrorCode.NotFound);

            //Handle server data
            if(split.Length > 1)
            {
                string nextUrl = path.Substring(serverId.Length + 1).TrimStart('/');

                //Authenticate the user
                DbUser user = await ApiTools.AuthenticateUser(ApiTools.GetBearerToken(e), true);

                //Look up the user's tribe by their steam ID
                int? tribeIdNullable = await server.TryGetTribeIdAsync(Program.connection, user.steam_id);
                bool hasTribe = tribeIdNullable.HasValue;
                if(!hasTribe)
                    throw new StandardError("You must be a part of this server to send API calls.", StandardErrorCode.NotPermitted);
                int tribeId = tribeIdNullable.Value;

                //Check if this is one of our URLs.
                if (nextUrl == "manage")
                    await ManageRequest.OnHttpRequest(e, server, user);
                else if (nextUrl.StartsWith("mods"))
                    await ServerMods.OnHttpRequest(e, server);
                else if (nextUrl.StartsWith("put_user_prefs"))
                    await ServerUpdateSavedPrefs.OnUserPrefsRequest(e, server, user);
                else if (nextUrl.StartsWith("put_dino_prefs/"))
                    await ServerUpdateSavedPrefs.OnTribeDinoPrefsRequest(e, server, user, tribeId, nextUrl.Substring("put_dino_prefs/".Length));
                else if (nextUrl.StartsWith("canvas/"))
                    await CanvasRequest.OnCanvasRequest(e, server, user, tribeId, nextUrl.Substring("canvas/".Length));
                else if (nextUrl.StartsWith("canvas"))
                    await CanvasRequest.OnListRequest(e, server, user, tribeId);
                else
                    throw new StandardError("Not Found", StandardErrorCode.NotFound);
            } else
            {
                //Return with some server info
                ArkServerReply se = new ArkServerReply(server, null);
                await Program.QuickWriteJsonToDoc(e, se);
            }
        }
    }
}
