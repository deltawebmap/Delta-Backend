using ArkWebMapGateway.ClientHandlers;
using ArkWebMapGatewayClient;
using ArkWebMapMasterServer.NetEntities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapGateway.Clients
{
    public class FrontendGatewayConnection : GatewayConnection
    {
        public string userId;
        public UsersMeReply user;
        public FrontendGatewayHandler handler;

        public static async Task<FrontendGatewayConnection> HandleIncomingConnection(Microsoft.AspNetCore.Http.HttpContext e, string version)
        {
            //Do authentication
            UsersMeReply user;
            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.Headers.Add("Authorization", "Bearer " + e.Request.Query["auth_token"]);
                    string s = wc.DownloadString("https://ark.romanport.com/api/users/@me/");
                    user = JsonConvert.DeserializeObject<UsersMeReply>(s);
                }
            } catch (Exception ex)
            {
                //Send auth failed.
                await Program.QuickWriteToDoc(e, "Not Authenticated.", "text/plain", 401);
                return null;
            }

            //Start
            FrontendGatewayConnection conn = new FrontendGatewayConnection
            {
                user = user,
                userId = user.id
                
            };
            conn.handler = new FrontendGatewayHandler(conn);

            //Run
            await conn.Run(e, () =>
            {
                //Ready
                //Add
                lock (ConnectionHolder.users)
                {
                    ConnectionHolder.users.Add(conn);
                }

                //Test
                /*MessageSender.SendMsgToTribe(new ArkWebMapGatewayClient.Messages.GatewayMessageBase
                {
                    opcode = ArkWebMapGatewayClient.GatewayMessageOpcode.None,
                    headers = new Dictionary<string, string>()
                }, "x5wyzx9myzU3AKkdzlWHBzAt", 1702654661);*/
            });

            return conn;
        }

        public override Task<bool> OnClose(WebSocketCloseStatus? status)
        {
            //Remove this from the list of clients
            lock (ConnectionHolder.users)
                ConnectionHolder.users.Remove(this);

            return base.OnClose(status);
        }

        public override Task<bool> OnMsg(string msg)
        {
            //Deserialize as base type to get the opcode
            GatewayMessageBase b = JsonConvert.DeserializeObject<GatewayMessageBase>(msg);

            //Now, let it be handled like normal.
            handler.HandleMsg(b.opcode, msg, this);

            //Return OK
            return Task.FromResult<bool>(true);
        }
    }
}
