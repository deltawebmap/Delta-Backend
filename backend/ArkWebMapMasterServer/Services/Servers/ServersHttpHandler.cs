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

            //Authenticate the user
            ArkUser user = Users.UsersHttpHandler.AuthenticateUser(e, true);
            if (user.GetServers().Where( x => x._id == server._id).Count() != 1)
                throw new StandardError("You must be a part of this server to send API calls.", StandardErrorCode.NotPermitted);

            //If there is content after this, proxy to this server. Else, return server info.
            if(split.Length > 1)
            {
                //Do proxy. Get full url
                string proxyUrl = path.Substring(serverId.Length + 1).TrimStart('/');

                //Check if this is one of our URLs.
                if (proxyUrl == "leave")
                {
                    //Leave
                    return LeaveServer.OnHttpRequest(e, server);
                }
                if(proxyUrl == "rename")
                {
                    //Rename
                    return RenameServer.OnHttpRequest(e, server);
                }

                

                try
                {
                    //Send
                    HttpContent content = new StreamContent(e.Request.Body);
                    HttpResponseMessage reply = server.OpenHttpRequest(content, "/api/" + proxyUrl + e.Request.QueryString, e.Request.Method, user);

                    //Set response
                    e.Response.StatusCode = (int)reply.StatusCode;
                    return reply.Content.CopyToAsync(e.Response.Body);
                } catch
                {
                    //Error out.
                    return Program.QuickWriteJsonToDoc(e, new ProxyError
                    {
                        endpoint_ip = server.latest_proxy_url,
                        endpoint_url = proxyUrl,
                        error_description = $"Could not proxy to this server. It is offline."
                    }, 502);
                }
                
            } else
            {
                //Return with some server info
                ArkServerReply se = new ArkServerReply(server, null);
                return Program.QuickWriteJsonToDoc(e, se);
            }
        }
    }
}
