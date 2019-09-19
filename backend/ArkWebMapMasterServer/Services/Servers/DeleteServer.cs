using ArkBridgeSharedEntities.Entities;
using ArkWebMapMasterServer.PresistEntities;
using LibDeltaSystem.Db.System;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Servers
{
    public class DeleteServer
    {
        public static Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, DbServer s, DbUser u)
        {
            //Validate that this user owns this server
            if(s.owner_uid != u.id)
            {
                throw new StandardError("You do not own this server and are not allowed to perform this action.", StandardErrorCode.NotPermitted);
            }

            //Delete
            s.DeleteAsync().GetAwaiter().GetResult();

            //Return ok
            return Program.QuickWriteStatusToDoc(e, true);
        }
    }
}
