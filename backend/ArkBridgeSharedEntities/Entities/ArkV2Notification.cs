using System;
using System.Collections.Generic;
using System.Text;

namespace ArkBridgeSharedEntities.Entities
{
    public class ArkV2Notification
    {
        public int uuid; //ID to use natively in Android. Ignored and replaced if sent by bridge.
        public string serverName; //Name of the server. Ignored and replaced if sent by the bridge.
        public string title; //Notification title
        public string content; //Notification content

        public ArkV2NotificationIconType iconType; //Smaller icon in the image
        public bool hasLargeIcon; //Has large icon. If false, next three entries are ignored
        public bool hasLargeIconSub; //Has smaller icon for the large icon
        public string largeIconUrl; //URL for the large icon
        public ArkV2NotificationLargeIconType largeIconSubType; //Type for the smaller sub icon in the large icon
    }

    public class ArkV2NotificationRequest
    {
        public int targetTribeId;
        public ArkV2Notification payload;
    }

    public enum ArkV2NotificationIconType
    {
        Default = 0,
        Tamed = 1, //NI
        Killed = 2, //NI
        BabyDino = 3, //NI
        Starving = 4, //NI
    }

    public enum ArkV2NotificationLargeIconType
    {

    }
}
