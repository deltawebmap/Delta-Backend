using System;
using System.Collections.Generic;
using System.Text;

namespace ArkBridgeSharedEntities.Requests
{
    public class UserNotificationRequest
    {
        public int tribeId;
        public TribeNotification notification;
    }

    public class TribeNotification 
    {
        public TribeNotificationType type;
        public Dictionary<string, string> data;
    }

    public enum TribeNotificationType
    {
        BabyDinoFoodLow,
        /*
         * data["foodItemsRemaining"] = (Float) The number of food energy remaining in the dino.
         * data["foodTimeRemaining"] = (Long) The time until the food is drained, in ticks, floored at zero.
         * data["dinoName"] = (String) The name of the dino
         * data["dinoClassname"] = (String) The classsname of the dino
         * data["dinoGender"] = (String) The pronoun of the dino. For example, He
         */
    }
}
