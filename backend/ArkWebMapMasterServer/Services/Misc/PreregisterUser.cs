using LibDeltaSystem.Db.System;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Misc
{
    public static class PreregisterUser
    {
        public static async Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e)
        {
            //Decode POST body
            RequestBody request = Program.DecodePostBody<RequestBody>(e);

            //Add to database
            var filterBuilder = Builders<DbPreregisteredUser>.Filter;
            var filter = filterBuilder.Eq("email", request.email);
            var results = await Program.connection.system_preregistered.FindAsync(filter);
            var r = await results.FirstOrDefaultAsync();
            if(r == null)
            {
                await Program.connection.system_preregistered.InsertOneAsync(new DbPreregisteredUser
                {
                    email = request.email,
                    time = DateTime.UtcNow
                });
            }

            //Write ok
            await Program.QuickWriteStatusToDoc(e, true);
        }

        class RequestBody
        {
            public string email;
        }
    }
}
