using ArkWebMapGatewayClient;
using ArkWebMapGatewayClient.Messages;
using ArkWebMapGatewayClient.Messages.Entities;
using ArkWebMapMasterServer.MirrorEntities;
using ArkWebMapMasterServer.PresistEntities;
using ArkWebMapMasterServer.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Mirror
{
    public static class MirrorService
    {
        public static Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e)
        {
            //Get the token and get the matching server
            string token = e.Request.Query["token"];

            //Do auth
            ArkMirrorToken auth = MirrorTokenTool.TryMatchToken(token);
            ArkServer server = null;
            if(auth != null)
                server = ArkWebMapMasterServer.Servers.ArkSlaveServerSetup.GetSlaveServerById(auth._id);

            //Log
            if (server == null)
                Console.WriteLine($"Client auth failed with token {token}");
            else
                Console.WriteLine($"Authenticated client event for server {server._id}");

            //Abort if failed
            if (server == null)
                return Program.QuickWriteStatusToDoc(e, false, 401);

            //Read and parse the body
            List<MirroredMessage> msgs = new List<MirroredMessage>();
            try
            {
                using (StreamReader sr = new StreamReader(e.Request.Body))
                {
                    MirrorProtocolReader reader = new MirrorProtocolReader(sr);
                    while (!sr.EndOfStream)
                    {
                        MirroredMessage msg = reader.ReadMessage();
                        if (msg.opcode == MirroredOpcode.EOS)
                            break;
                        else
                            msgs.Add(msg);
                    }
                }
            } catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message + ex.StackTrace);
                return Program.QuickWriteStatusToDoc(e, false);
            }

            //Handle
            try
            {
                HandleMsgs(server, auth, msgs);
            } catch (Exception ex)
            {
                Console.WriteLine($"Failed processing messages for events: {ex.Message}{ex.StackTrace}");
            }

            //Return OK, but Ark won't care
            return Program.QuickWriteStatusToDoc(e, true);
        }

        private static void HandleMsgs(ArkServer server, ArkMirrorToken auth, List<MirroredMessage> msgs)
        {
            //Sort position updates by tribe and send them to the GATEWAY
            Dictionary<int, List<UpdateEntityRealtimePosition>> gatewayMessages = new Dictionary<int, List<UpdateEntityRealtimePosition>>();
            List<int> tribeIds = new List<int>();
            foreach (var m in msgs)
            {
                var response = m.ProcessMsg(server, auth);
                if(response != null)
                {
                    int tribeId = response.Item2;
                    if (gatewayMessages.ContainsKey(tribeId))
                        gatewayMessages[tribeId].Add(response.Item1);
                    else
                    {
                        gatewayMessages.Add(tribeId, new List<UpdateEntityRealtimePosition> { response.Item1 });
                        tribeIds.Add(tribeId);
                    }
                }
            }

            //Send the position updates to the tribes
            for (int i = 0; i < tribeIds.Count; i++)
            {
                GatewayActionTool.SendActionToTribe(new MessageUpdateRealtimePos
                {
                    opcode = GatewayMessageOpcode.RealtimeMapMovement,
                    updates = gatewayMessages[tribeIds[i]]
                }, tribeIds[i], server._id);
            }
        }
    }
}
