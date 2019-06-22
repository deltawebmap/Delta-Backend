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

            /*using (StreamReader sr = new StreamReader(e.Request.Body))
                Console.WriteLine(sr.ReadToEnd());
            return Program.QuickWriteStatusToDoc(e, true);*/

            //Do auth
            ArkMirrorToken auth = MirrorTokenTool.TryMatchToken(token);
            ArkServer server = ArkWebMapMasterServer.Servers.ArkSlaveServerSetup.GetSlaveServerById(auth._id);

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

            //Sort position updates by tribe and send them to the GATEWAY
            Dictionary<int, List<UpdateEntityRealtimePosition>> gatewayMessages = new Dictionary<int, List<UpdateEntityRealtimePosition>>();
            List<int> tribeIds = new List<int>();
            foreach(var m in msgs)
            {
                var response = m.ProcessMsg(server, auth);
                int tribeId = response.Item2;
                if (gatewayMessages.ContainsKey(tribeId))
                    gatewayMessages[tribeId].Add(response.Item1);
                else
                {
                    gatewayMessages.Add(tribeId, new List<UpdateEntityRealtimePosition> { response.Item1 });
                    tribeIds.Add(tribeId);
                }
            }

            //Send the position updates to the tribes
            for(int i = 0; i<tribeIds.Count; i++)
            {
                GatewayActionTool.SendActionToTribe(new MessageUpdateRealtimePos
                {
                    opcode = GatewayMessageOpcode.RealtimeMapMovement,
                    updates = gatewayMessages[tribeIds[i]]
                }, tribeIds[i], server._id);
            }

            //Return OK, but Ark won't care
            return Program.QuickWriteStatusToDoc(e, true);
        }
    }
}
