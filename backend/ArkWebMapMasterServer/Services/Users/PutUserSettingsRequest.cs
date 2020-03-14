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
            //Verify method
            if (GetMethod() != LibDeltaSystem.WebFramework.Entities.DeltaCommonHTTPMethod.POST)
                throw new StandardError("Only POST requests are valid here.", StandardErrorCode.BadMethod);

            //Decode
            var settings = await DecodePOSTBody<DbUserSettings>();

            //Update
            user.user_settings = settings;
            if (user.user_settings == null)
            {
                throw new StandardError("Cannot set user settings to null.", StandardErrorCode.InvalidInput);
            }
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
