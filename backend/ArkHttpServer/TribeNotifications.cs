using ArkBridgeSharedEntities.Requests;
using ArkSaveEditor.World.WorldTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkHttpServer
{
    public static class TribeNotifications
    {
        public static void SendTribeDinoFoodStatus(ArkDinosaur dino, int tribeId)
        {
            //Build the request.
            TribeNotification n = new TribeNotification
            {
                data = new Dictionary<string, string>(),
                type = TribeNotificationType.BabyDinoFoodLow
            };

            n.data.Add("foodItemsRemaining", "0"); //TEMP
            n.data.Add("foodTimeRemaining", "0"); //TEMP
            n.data.Add("dinoName", dino.tamedName);
            n.data.Add("dinoClassname", dino.dino_entry.screen_name);
            if (dino.isFemale)
                n.data.Add("dinoGender", "She");
            else
                n.data.Add("dinoGender", "He");

            //Send
            PrivateSendTribeNotification(n, tribeId);
        }

        private static void PrivateSendTribeNotification(TribeNotification n, int tribeId)
        {
            ArkWebServer.tribeNotificationCode(tribeId, n);
        }
    }
}
