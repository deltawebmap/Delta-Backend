using LibDeltaSystem.Db.System;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Users
{
    public static class NotificationTokenRequest
    {
        public static async Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, DbUser u)
        {
            //Decode POST body
            var body = Program.DecodePostBody<RequestData>(e);

            //Add token
            if (u.notification_tokens == null)
                u.notification_tokens = new List<string>();
            if (!u.notification_tokens.Contains(body.token))
                u.notification_tokens.Add(body.token);

            //Return ok
            await Program.QuickWriteStatusToDoc(e, true);
        }

        class RequestData
        {
            public string token;
        }
    }
}
