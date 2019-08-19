using ArkBridgeSharedEntities.Entities;
using ArkWebMapMasterServer.PresistEntities;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Users
{
    public class UsersHttpHandler
    {
        public static ArkUser AuthenticateUser(Microsoft.AspNetCore.Http.HttpContext e, bool required)
        {
            return AuthenticateUser(e, required, out string t);
        }

        public static string GetAuthToken(Microsoft.AspNetCore.Http.HttpContext e)
        {
            string token = null;
            if (e.Request.Headers.ContainsKey("Authorization"))
            {
                //Read Authorization header
                token = e.Request.Headers["Authorization"];
                if (token.StartsWith("Bearer "))
                    token = token.Substring("Bearer ".Length);
            }
            return token;
        }

        public static ArkUser AuthenticateUser(Microsoft.AspNetCore.Http.HttpContext e, bool required, out string token)
        {
            token = GetAuthToken(e);
            if (token == null && required)
                throw new StandardError("No Auth Token Provided.", StandardErrorCode.AuthRequired);
            ArkUser user = ArkWebMapMasterServer.Users.UserTokens.ValidateUserToken(token);
            if (user == null && required)
                throw new StandardError("You're not signed in.", StandardErrorCode.AuthRequired);
            return user;
        }

        public static Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, string path)
        {
            //Check if this is a demo user
            if(GetAuthToken(e) == "demo-user")
            {
                //This is a demo user. ALWAYS provide it with the placeholder users/me
                NetEntities.UsersMeReply usersme = new NetEntities.UsersMeReply();
                usersme.MakeDummyUsersMe();
                return Program.QuickWriteJsonToDoc(e, usersme);
            }
            
            //Every path here requies authentication. Do it.
            ArkUser user = AuthenticateUser(e, true, out string userToken);

            //Check path
            if (path.StartsWith("@me/server_wizard/start"))
            {
                //Pass onto server gen
                return Misc.ArkSetupProxy.OnCreateProxySessionRequest(e, user);
            }
            if(path.StartsWith("@me/logout"))
            {
                //Delete browser cookie
                Auth.AuthHttpHandler.SetAuthCookie(e, "");
                return Program.QuickWriteStatusToDoc(e, true);
            }
            if (path.StartsWith("@me/server_wizard/test_tcp_connection"))
            {
                //Quickly connect to the TCP server and exchange data
                return OnTestTcpRequest(e, user);
            }
            if(path.StartsWith("@me/servers/add_ignore/"))
            {
                //Add this server to the ignore list.
                string serverId = e.Request.Query["id"];
                if (user.hidden_servers == null)
                    user.hidden_servers = new List<string>();
                if (!user.hidden_servers.Contains(serverId))
                    user.hidden_servers.Add(serverId);
                user.Update();
                return Program.QuickWriteStatusToDoc(e, true);
            }
            if (path.StartsWith("@me/servers/remove_ignore/"))
            {
                //Add this server to the ignore list.
                string serverId = e.Request.Query["id"];
                if (user.hidden_servers == null)
                    user.hidden_servers = new List<string>();
                if (user.hidden_servers.Contains(serverId))
                    user.hidden_servers.Remove(serverId);
                user.Update();
                return Program.QuickWriteStatusToDoc(e, true);
            }
            if (path.StartsWith("@me/servers/remove_ignore_mass/"))
            {
                //Loop through servers
                string[] serverIds = e.Request.Query["ids"].ToString().Split(',');
                if (user.hidden_servers == null)
                    user.hidden_servers = new List<string>();
                foreach (string s in serverIds)
                {
                    if(user.hidden_servers.Contains(s))
                        user.hidden_servers.Remove(s);
                }
                user.Update();
                return Program.QuickWriteStatusToDoc(e, true);
            }
            if(path.StartsWith("@me/servers/change_notifications"))
            {
                return EditServerNotifications.OnHttpRequest(e, user);
            }
            if(path == "@me/notification_token")
            {
                //Put notification token.
                return PostedNotificationToken.OnHttpRequest(e, user);
            }
            if (path == "@me/hub")
            {
                return HubService.OnHttpRequest(e, user);
            }
            if (path == "@me/report_issue")
            {
                IssueCreator.OnHttpRequest(e, user).GetAwaiter().GetResult();
                return null;
            }
            if (path == "@me/tokens/@this/devalidate")
            {
                return TokenDevalidateService.OnSingleDevalidate(e, user, userToken);
            }
            if (path == "@me/tokens/@all/devalidate")
            {
                return TokenDevalidateService.OnAllDevalidate(e, user);
            }
            if(path == "@me/user_settings")
            {
                //Verify method
                if (Program.FindRequestMethod(e) != RequestHttpMethod.post)
                    throw new StandardError("Only POST or post requests are valid here.", StandardErrorCode.BadMethod);
                
                //Update
                user.user_settings = Program.DecodePostBody<ArkUserSettings>(e);
                user.Update();
                return Program.QuickWriteStatusToDoc(e, true);
            }
            if (path.StartsWith("@me/"))
            {
                //Requested user info
                return UsersMe.OnHttpRequest(e, user);
            }

            //Not found
            throw new StandardError("Not Found", StandardErrorCode.NotFound);
        }

        public static Task OnTestTcpRequest(Microsoft.AspNetCore.Http.HttpContext e, ArkUser u)
        {
            //Pull IP and port from query
            IPAddress ip = IPAddress.Parse(e.Request.Query["ip"]);
            int port = int.Parse(e.Request.Query["port"]);
            IPEndPoint endpoint = new IPEndPoint(ip, port);

            //Connect 
            try
            {
                TcpClient tcp = new TcpClient();
                tcp.SendTimeout = 5000;
                tcp.ReceiveTimeout = 5000;
                tcp.Connect(endpoint);
                if (!tcp.Connected)
                    throw new Exception(); //Not connected, escape

                //Send over a messsage and wait for a reply
                tcp.Client.Send(new byte[] { 0x50, 0x69, 0x6E, 0x67, 0x21 });

                //Open a buffer for the reply
                byte[] buf = new byte[5];
                tcp.Client.Receive(buf);

                //Validate reply
                byte[] match = new byte[] { 0x50, 0x6F, 0x6E, 0x67, 0x21 };
                if(Program.CompareByteArrays(match, buf))
                {
                    //Passed
                    return Program.QuickWriteJsonToDoc(e, new ArkBridgeSharedEntities.Entities.TrueFalseReply
                    {
                        ok = true
                    });
                } else
                {
                    throw new Exception();
                }
            } catch
            {
                //Failed
                return Program.QuickWriteJsonToDoc(e, new ArkBridgeSharedEntities.Entities.TrueFalseReply
                {
                    ok = false
                });
            }
        }
    }
}
