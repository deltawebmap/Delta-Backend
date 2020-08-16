using LibDeltaSystem;
using LibDeltaSystem.Db.System;
using LibDeltaSystem.WebFramework.ServiceTemplates;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Users
{
    public class PutUserSettingsRequest : UserAuthDeltaService
    {
        public PutUserSettingsRequest(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public override async Task OnRequest()
        {
            //Decode
            var settings = await ReadPOSTContentChecked<DbUserSettings>();
            if (settings == null)
                return;

            //Update
            await user.UpdateAsync(Program.connection);

            //Return response
            await WriteJSON(settings);
        }

        public override async Task<bool> SetArgs(Dictionary<string, string> args)
        {
            return true;
        }
    }
}
