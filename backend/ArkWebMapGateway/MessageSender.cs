using ArkBridgeSharedEntities.Entities;
using ArkWebMapGatewayClient;
using ArkWebMapGatewayClient.Messages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ArkWebMapGateway
{
    public static class MessageSender
    {
        public static void SendMsgToUserAWMId(GatewayMessageBase msg, string id)
        {
            //Loop through and send
            lock (ConnectionHolder.users)
            {
                for(int i = 0; i<ConnectionHolder.users.Count; i+=1)
                {
                    if (ConnectionHolder.users[i].userId == id)
                        InternalQueueMsg(msg, ConnectionHolder.users[i]);
                }
            }
        }

        public static void SendMsgToUserSteamID(GatewayMessageBase msg, string steamId)
        {
            //Loop through and send
            lock (ConnectionHolder.users)
            {
                for (int i = 0; i < ConnectionHolder.users.Count; i += 1)
                {
                    if (ConnectionHolder.users[i].user.steam_id == steamId)
                        InternalQueueMsg(msg, ConnectionHolder.users[i]);
                }
            }
        }

        public static void SendMsgToTribe(GatewayMessageBase msg, string serverId, int tribeId)
        {
            //Set headers
            if (!msg.headers.ContainsKey("tribe_id"))
                msg.headers.Add("tribe_id", tribeId.ToString());
            if (!msg.headers.ContainsKey("server_id"))
                msg.headers.Add("server_id", serverId);

            //Get the server data
            List<ArkSlaveReport_PlayerAccount> serverMembers = ServerDataHolder.GetServerMembers(serverId);
            var tribeMembers = serverMembers.Where(x => x.player_tribe_id == tribeId);
            foreach (var t in tribeMembers)
                SendMsgToUserSteamID(msg, t.player_steam_id);
        }

        private static void InternalQueueMsg(GatewayMessageBase msg, GatewayConnection conn)
        {
            conn.SendMsg(msg);
        }
    }
}
