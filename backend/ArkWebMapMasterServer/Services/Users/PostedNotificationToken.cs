using ArkWebMapMasterServer.NetEntities;
using ArkWebMapMasterServer.PresistEntities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Users
{
    class PostedNotificationToken
    {
        public static Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, ArkUser u)
        {
            //Decode the payload and get the notification token.
            PostNotificationToken payload = Program.DecodePostBody<PostNotificationToken>(e);

            //Check
            if(u.notification_tokens == null)
                u.notification_tokens = new List<string>();

            //Add
            if (!u.notification_tokens.Contains(payload.token))
            {
                u.notification_tokens.Add(payload.token);
                u.Update();
            }

            //Return OK
            return Program.QuickWriteStatusToDoc(e, true);
        }
    }
}
