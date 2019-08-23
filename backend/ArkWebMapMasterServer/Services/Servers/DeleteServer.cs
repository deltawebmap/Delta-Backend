using ArkBridgeSharedEntities.Entities;
using ArkWebMapMasterServer.PresistEntities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Servers
{
    public class DeleteServer
    {
        public static Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, ArkServer s, ArkUser u)
        {
            //Validate that this user owns this server
            if(s.owner_uid != u._id)
            {
                throw new StandardError("You do not own this server and are not allowed to perform this action.", StandardErrorCode.NotPermitted);
            }

            //Delete
            s.DeleteServer();

            //Return ok
            return Program.QuickWriteStatusToDoc(e, true);
        }
    }
}
