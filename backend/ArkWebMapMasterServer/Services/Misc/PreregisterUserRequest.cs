using LibDeltaSystem;
using LibDeltaSystem.Db.System;
using LibDeltaSystem.WebFramework.ServiceTemplates;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Misc
{
    public class PreregisterUserRequest : BasicDeltaService
    {
        public PreregisterUserRequest(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public override async Task OnRequest()
        {
            //Decode POST body
            RequestBody request = Program.DecodePostBody<RequestBody>(e);

            //Add to database
            var filterBuilder = Builders<DbPreregisteredUser>.Filter;
            var filter = filterBuilder.Eq("email", request.email);
            var results = await Program.connection.system_preregistered.FindAsync(filter);
            var r = await results.FirstOrDefaultAsync();
            if (r == null)
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
