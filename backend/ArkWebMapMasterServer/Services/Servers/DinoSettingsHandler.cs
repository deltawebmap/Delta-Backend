using ArkBridgeSharedEntities.Entities;
using ArkBridgeSharedEntities.Entities.Master;
using ArkWebMapGatewayClient.Messages;
using ArkWebMapMasterServer.PresistEntities;
using ArkWebMapMasterServer.Tools;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Servers
{
    public static class DinoSettingsHandler
    {
        public static Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, string next, ArkUser user, ArkServer s, bool hasTribe, int tribeId)
        {
            //Stop if we're not in a tribe
            if (!hasTribe)
                throw new StandardError("No Tribe Found", StandardErrorCode.NotPermitted);

            //Parse dino ID
            if(!ulong.TryParse(next, out ulong dinoId))
                throw new StandardError("Dino ID Invalid", StandardErrorCode.MissingRequiredArg);

            //Find
            string query = TribeDinoSettingsTool.CreateKey(s, tribeId, dinoId);
            var collec = TribeDinoSettingsTool.GetCollection();
            DinoTribeSettings d = collec.FindById(query);

            //Create if it does not exist
            if(d == null)
            {
                d = new DinoTribeSettings
                {
                    dino_id = dinoId,
                    server_id = s._id,
                    tribe_id = tribeId,
                    _id = query,
                    notes = "",
                    group_color = -1
                };
                collec.Insert(d);
            }
            return ArrayActionsHandler.OnHttpRequestCustomFind(e, user, d, SelectGet, SelectPost, null);
        }

        public static Task SelectGet(Microsoft.AspNetCore.Http.HttpContext e, DinoTribeSettings dino, ArkUser user)
        {
            return Program.QuickWriteJsonToDoc(e, dino);
        }

        public static Task SelectPost(Microsoft.AspNetCore.Http.HttpContext e, DinoTribeSettings dino, ArkUser user)
        {
            //Read payload and update
            DinoTribeSettings payload = Program.DecodePostBody<DinoTribeSettings>(e);
            if (payload.notes != null)
                dino.notes = payload.notes;
            if (payload.group_color != null)
                dino.group_color = payload.group_color;

            //Update
            TribeDinoSettingsTool.GetCollection().Update(dino);

            //Send on gateway
            GatewayActionTool.SendActionToTribe(new MessageUpdateTribeDinoSettings
            {
                data = dino,
                headers = new Dictionary<string, string>(),
                opcode = ArkWebMapGatewayClient.GatewayMessageOpcode.MessageUpdateTribeDinoSettings
            }, dino.tribe_id, dino.server_id);

            //Write updated
            return Program.QuickWriteJsonToDoc(e, dino);
        }
    }
}
