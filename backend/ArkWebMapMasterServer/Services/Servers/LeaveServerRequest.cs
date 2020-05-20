using ArkWebMapMasterServer.ServiceTemplates;
using LibDeltaSystem;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Servers
{
    public class LeaveServerRequest : MasterTribeServiceTemplate
    {
        public LeaveServerRequest(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public override async Task OnRequest()
        {
            //Confirm method
            if(GetMethod() != LibDeltaSystem.WebFramework.Entities.DeltaCommonHTTPMethod.POST)
            {
                await WriteString("Only POST expected!", "text/plain", 400);
                return;
            }

            //Make sure we are not the owner
            if (server.owner_uid == user._id)
            {
                await WriteString("The server owner cannot leave their own server. Please delete it instead.", "text/plain", 400);
                return;
            }

            //Get player profile
            await server.DeleteUserPlayerProfile(conn, user);

            //Also remove them from the list of admins
            await server.RemoveAdmin(conn, user);

            await WriteStatus(true);
        }
    }
}
