using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapMasterServer.NetEntities
{
    public class FirebaseNotification
    {
        public string title;
        public string body;
    }

    public class FirebaseNotificationRequest
    {
        public FirebaseNotification notification;
        public string to;
        public string priority;
    }
}
