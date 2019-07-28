using ArkWebMapAnalytics.NetEntities;
using ArkWebMapAnalytics.PersistEntities;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapAnalytics.Services
{
    public static class ActionsService
    {
        public static async Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e)
        {
            //Get the payload
            ActionPayload payload = Program.DecodePostBody<ActionPayload>(e);

            //Verify
            if (payload.access_token == null || payload.client_details == null || payload.client_name == null || payload.client_version == null || payload.client_view == null || payload.extras == null || payload.topic == null)
                throw new StandardError("Missing required payload data.", 400);

            //Authenticate
            string uid = UserAuthenticator.GetUserIdFromToken(payload.access_token);

            //Generate a unique ID
            string id = Program.GenerateRandomID();
            LiteCollection<ActionEntry> collec = Program.db.GetCollection<ActionEntry>("actions");
            while (collec.FindById(id) != null)
                id = Program.GenerateRandomID();

            //Make into ActionEntry
            ActionEntry entry = new ActionEntry
            {
                access_token = payload.access_token,
                client_details = payload.client_details,
                client_name = payload.client_name,
                client_version = payload.client_version,
                client_view = payload.client_view,
                extras = payload.extras,
                logger_version = 1,
                server_id = payload.server_id,
                server_online = payload.server_online,
                time = DateTime.UtcNow.Ticks,
                topic = payload.topic,
                user_id = uid,
                _id = id
            };

            //Add
            collec.Insert(entry);

            //Write response
            await Program.QuickWriteJsonToDoc(e, new LogResponse
            {
                id = id,
                is_auth = uid != null
            });
        }
    }
}
