using ArkWebMapMasterServer.NetEntities;
using ArkWebMapMasterServer.PresistEntities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer
{
    public static class MobileDeviceNotifications
    {
        public static void SendNotificationToTribe(ArkServer s, int tribeId, FirebaseNotification message)
        {
            //Grab users
            List<ArkUser> users = TribeTool.GetTribePlayers(s, tribeId);

            //Send to each user
            foreach(ArkUser u in users)
            {
                SendUserNotification(u, message);
            }
        }

        public static void SendUserNotification(ArkUser u, FirebaseNotification message)
        {
            //Loop through all tokens this user has and send them a notification.
            if (u.notification_tokens == null)
                return; //Skip. There are no tokens.

            foreach(string token in u.notification_tokens)
            {
                SendMessageToToken(token, message).GetAwaiter().GetResult();
            }
        }

        private static Task<WebResponse> SendMessageToToken(string token, FirebaseNotification message)
        {
            //Create request
            FirebaseNotificationRequest request_message = new FirebaseNotificationRequest
            {
                notification = message,
                to = token,
                priority = "HIGH"
            };

            //Send
            try
            {
                var request = (HttpWebRequest)WebRequest.Create("https://fcm.googleapis.com/fcm/send");
                var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(request_message));
                request.Method = "POST";
                request.ContentType = "application/json";
                request.ContentLength = data.Length;
                request.Headers.Add("Authorization", "key=" + Program.config.firebase_cloud_messages_api_key);
                using (var stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }

                return request.GetResponseAsync();
            } catch
            {
                Program.Log("Failed to send notification. Ignoring...", ConsoleColor.Yellow);
                return new Task<WebResponse>(null);
            }
        }
    }
}
