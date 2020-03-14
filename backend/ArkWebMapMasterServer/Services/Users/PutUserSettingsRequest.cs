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
            if (Program.FindRequestMethod(e) != RequestHttpMethod.post)
                throw new StandardError("Only POST or post requests are valid here.", StandardErrorCode.BadMethod);

            //Update
            user.user_settings = Program.DecodePostBody<DbUserSettings>(e);
            if (user.user_settings == null)
            {
                throw new StandardError("Cannot set user settings to null.", StandardErrorCode.InvalidInput);
            }
            await user.UpdateAsync(Program.connection);

            //Return response
            await Program.QuickWriteStatusToDoc(e, true);
        }

        public override async Task<bool> SetArgs(Dictionary<string, string> args)
        {
            return true;
        }
    }
}
