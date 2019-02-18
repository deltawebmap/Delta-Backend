using ArkBridgeSharedEntities.Entities;
using ArkWebMapMasterServer.NetEntities;
using ArkWebMapMasterServer.PresistEntities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Users
{
    public class EditServerNotifications
    {
        public static Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, ArkUser u)
        {
            //Decode the payload
            EditServerNotificationsPayload payload = Program.DecodePostBody<EditServerNotificationsPayload>(e);

            //Convert the channel strings to channel IDs
            List<ArkNotificationChannel> channels = new List<ArkNotificationChannel>();
            foreach(string s in payload.channels)
            {
                if (Enum.TryParse<ArkNotificationChannel>(s, out ArkNotificationChannel c))
                    channels.Add(c);
            }

            //Set server ID
            if (u.enabled_notifications == null)
                u.enabled_notifications = new Dictionary<string, List<ArkNotificationChannel>>();
            if (u.enabled_notifications.ContainsKey(payload.id))
                u.enabled_notifications[payload.id] = channels;
            else
                u.enabled_notifications.Add(payload.id, channels);

            //Save
            u.Update();

            //Return OK
            return Program.QuickWriteStatusToDoc(e, true);
        }
    }
}
