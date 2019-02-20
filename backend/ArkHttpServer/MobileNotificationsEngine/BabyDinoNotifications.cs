using ArkSaveEditor.World;
using ArkSaveEditor.World.WorldTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArkHttpServer.MobileNotificationsEngine
{
    public class BabyDinoNotifications
    {
        public static void CheckBabyDinos()
        {
            //Grab the Ark world
            ArkWorld world = WorldLoader.GetWorld(out DateTime lastUpdateTime);
            double timeOffsetSeconds = (DateTime.UtcNow - lastUpdateTime).TotalSeconds;

            //Enumerate all baby dinos
            var dinosToCheck = world.dinos.Where(x => x.isBaby == true && x.babyAge < 1);
            foreach(ArkDinosaur dino in dinosToCheck)
            {
                //Calculate dino stats
                BabyDinoStatsCalculator.BabyDinoInfoPackage stats = BabyDinoStatsCalculator.GetFullDinoInfo(new Entities.ArkDinoReply(dino, world), timeOffsetSeconds);
                TimeSpan timeToDepletion = TimeSpan.FromMilliseconds(stats.timeToDepletionMs);

                if(timeToDepletion.TotalMinutes < 30)
                {
                    //Send a notification.
                    TribeNotifications.SendTribeDinoFoodStatus(dino, dino.tribeId, stats);
                } else
                {
                    //All clear. Remove if needed
                    TribeNotifications.DevalidateDuplicateDinoNotification(dino);
                }
            }
        }
    }
}
