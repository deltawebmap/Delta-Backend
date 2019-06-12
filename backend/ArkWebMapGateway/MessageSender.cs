using ArkBridgeSharedEntities.Entities;
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
        private static Queue<QueuedMessage> txQueue;
        private static Thread bgThread;

        public static void StartBgThread()
        {
            txQueue = new Queue<QueuedMessage>();
            bgThread = new Thread(TxBgThread);
            bgThread.IsBackground = true;
            bgThread.Start();
        }

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
            //Get the server data
            List<ArkSlaveReport_PlayerAccount> serverMembers = ServerDataHolder.GetServerMembers(serverId);
            var tribeMembers = serverMembers.Where(x => x.player_tribe_id == tribeId);
            foreach (var t in tribeMembers)
                SendMsgToUserSteamID(msg, t.player_steam_id);
        }

        private static void InternalQueueMsg(GatewayMessageBase msg, GatewayConnection conn)
        {
            txQueue.Enqueue(new QueuedMessage
            {
                client = conn,
                msg = msg
            });
        }

        private static void TxBgThread()
        {
            while(true)
            {
                if (txQueue.TryDequeue(out QueuedMessage msg))
                {
                    try
                    {
                        //Serialize
                        string payload = JsonConvert.SerializeObject(msg.msg);
                        msg.client.SendMsg(payload).GetAwaiter().GetResult();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Failed to send msg with error: " + ex.Message + ex.StackTrace);
                    }
                }
            }
        }
    }

    class QueuedMessage
    {
        public GatewayMessageBase msg;
        public GatewayConnection client;
    }
}
