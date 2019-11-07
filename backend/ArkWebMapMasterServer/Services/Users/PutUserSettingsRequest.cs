using LibDeltaSystem.Db.System;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Users
{
    public static class PutUserSettingsRequest
    {
        public static async Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, DbUser user)
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
            await user.UpdateAsync();

            //Return response
            await Program.QuickWriteStatusToDoc(e, true);
        }
    }
}
