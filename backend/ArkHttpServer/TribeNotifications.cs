using ArkBridgeSharedEntities.Requests;
using ArkSaveEditor.World.WorldTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkHttpServer
{
    public static class TribeNotifications
    {
        private static Dictionary<string, int> warning_dino_ids = new Dictionary<string, int>(); //Key: Dino ID, Value: Severity

        public static void SendTribeDinoFoodStatus(ArkDinosaur dino, int tribeId, BabyDinoStatsCalculator.BabyDinoInfoPackage stats)
        {
            //Determine a severity level to prevent duplicate notifications.
            int severity = 0;
            TimeSpan timeRemaining = TimeSpan.FromMilliseconds(stats.timeToDepletionMs);
            if (timeRemaining.TotalMinutes < 8)
                severity = 1;
            if (timeRemaining.TotalMinutes < 1)
                severity = 2;

            //Check if we've sent this notification before and add it if we haven't.
            string dinoId = dino.dinosaurId.ToString();
            if (warning_dino_ids.ContainsKey(dinoId))
            {
                if (warning_dino_ids[dinoId] == severity)
                    return;
                else
                    warning_dino_ids[dinoId] = severity;
            } else
            {
                warning_dino_ids.Add(dinoId, severity);
            }

            //Build the request.
            TribeNotification n = new TribeNotification
            {
                data = new Dictionary<string, string>(),
                type = TribeNotificationType.BabyDinoFoodLow
            };

            n.data.Add("foodItemsRemaining", stats.totalCurrentFood.ToString());
            n.data.Add("foodTimeRemaining", stats.timeToDepletionMs.ToString());
            n.data.Add("dinoName", dino.tamedName);
            n.data.Add("dinoClassname", dino.dino_entry.screen_name);
            if (dino.isFemale)
                n.data.Add("dinoGender", "She");
            else
                n.data.Add("dinoGender", "He");

            //Send
            PrivateSendTribeNotification(n, tribeId);
        }

        /// <summary>
        /// Sent to remove the dino from the list once it's fed to allow new notificaitons.
        /// </summary>
        /// <param name="dino"></param>
        public static void DevalidateDuplicateDinoNotification(ArkDinosaur dino)
        {
            string dinoId = dino.dinosaurId.ToString();
            if (warning_dino_ids.ContainsKey(dinoId))
            {
                warning_dino_ids.Remove(dinoId);
            }
        }

        private static void PrivateSendTribeNotification(TribeNotification n, int tribeId)
        {
            ArkWebServer.tribeNotificationCode(tribeId, n);
        }
    }
}
