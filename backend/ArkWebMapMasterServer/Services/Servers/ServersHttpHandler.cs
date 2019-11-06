﻿using ArkWebMapMasterServer.NetEntities;
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
using LibDeltaSystem.Db.System;

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
            DbServer server = ArkWebMapMasterServer.Servers.ArkSlaveServerSetup.GetSlaveServerById(serverId);

            //If there is content after this, proxy to this server. Else, return server info.
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
                {
                    //Leave
                    return DeleteServer.OnHttpRequest(e, server, user);
                }
                if(nextUrl == "edit")
                {
                    //Rename
                    return EditServerListing.OnHttpRequest(e, server);
                }
                if(nextUrl.StartsWith("maps"))
                {
                    if (!hasTribe)
                        throw new StandardError("Could not find player tribe.", StandardErrorCode.NotPermitted);
                    return ServerMaps.OnHttpRequest(e, server, tribeId, nextUrl.Substring("maps".Length));
                }
                if(nextUrl.StartsWith("mods"))
                {
                    return ServerMods.OnHttpRequest(e, server);
                }
                if (nextUrl.StartsWith("put_user_prefs"))
                {
                    return ServerUpdateSavedPrefs.OnUserPrefsRequest(e, server, user);
                }
                if (nextUrl.StartsWith("put_dino_prefs/"))
                {
                    return ServerUpdateSavedPrefs.OnTribeDinoPrefsRequest(e, server, tribeId, nextUrl.Substring("put_dino_prefs/".Length));
                }
                if (nextUrl.StartsWith("test_rp"))
                {
                    return Program.connection.SendPushNotificationToTribe(server, tribeId, "Test!");
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
