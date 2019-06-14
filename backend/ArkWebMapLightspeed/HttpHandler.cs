using ArkWebMapMasterServer.NetEntities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapLightspeed
{
    public static class HttpHandler
    {
        public static async Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e)
        {
            //If this is a WebSocket request, assume this is for a server.
            if(e.WebSockets.IsWebSocketRequest)
            {
                await SubserverConnection.AcceptConnection(e);
            } else
            {
                //Assume this is a proxy request. Find the server ID and proxy the request.
                string path = e.Request.Path.ToString();
                string[] split = path.Split('/');
                if(split.Length <= 2)
                {
                    await Program.QuickWriteToDoc(e, "Invalid URL structure.", "text/plain", 404);
                    return;
                }
                string id = split[1];
                string next = path.Substring(2 + id.Length);
                await HandleProxyRequest(e, id, next);

            }
        }

        private static async Task HandleProxyRequest(Microsoft.AspNetCore.Http.HttpContext e, string id, string next)
        {
            //Authenticate the user. First, obtain the access token
            UsersMeReply user = null;
            if(e.Request.Headers.ContainsKey("authorization"))
            {
                string authHeader = e.Request.Headers["authorization"];
                if(authHeader.StartsWith("Bearer "))
                {
                    string token = authHeader.Substring("Bearer ".Length);

                    //Now that we have obtained a token, check if we have it cached.
                    if (ConnectionHolder.cachedTokens.ContainsKey(token))
                        user = ConnectionHolder.cachedTokens[token];
                    else
                    {
                        //We'll need to authenticate the user
                        try
                        {
                            using (WebClient wc = new WebClient())
                            {
                                wc.Headers.Add("Authorization", "Bearer " + token);
                                string content = wc.DownloadString("https://ark.romanport.com/api/users/@me/");
                                user = JsonConvert.DeserializeObject<UsersMeReply>(content);
                                if (user != null)
                                    ConnectionHolder.cachedTokens.TryAdd(token, user);
                            }
                        }
                        catch { }
                    }
                }
            }

            //If we're not authenticated, stop
            if(user == null)
            {
                await Program.QuickWriteToDoc(e, "Not Authenticated.", "text/plain", 402);
                return;
            }

            //Check if this is a server this user is part of
            if(user.servers.Where( x => x.id == id).Count() == 0)
            {
                await Program.QuickWriteToDoc(e, "You are not a member of this server.", "text/plain", 401);
                return;
            }

            //Try and get this server from connected servers
            if(!ConnectionHolder.serverConnections.TryGetValue(id, out SubserverConnection connection))
            {
                await Program.QuickWriteToDoc(e, "This server is not connected to the ArkWebMap LIGHTSPEED network.", "text/plain", 502);
                return;
            }

            //Let the websocket handle the request
        }
    }
}
