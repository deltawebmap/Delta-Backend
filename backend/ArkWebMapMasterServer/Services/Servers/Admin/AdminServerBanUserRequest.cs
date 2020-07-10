using ArkWebMapMasterServer.ServiceTemplates;
using LibDeltaSystem;
using LibDeltaSystem.WebFramework.ServiceTemplates;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Servers.Admin
{
    public class AdminServerBanUserRequest : MasterArkRpcService
    {
        public AdminServerBanUserRequest(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public override async Task<RpcCommand?> BuildArkRpcEvent()
        {
            //Decode data
            var s = await ReadPOSTContentChecked<RequestData>();
            if (s == null)
                return null;

            //Attempt to get the user data
            var profile = await conn.GetUserBySteamIdAsync(s.steam_id);

            //Check if this user is an admin or owns this
            if(profile != null)
            {
                if (server.CheckIsUserAdmin(profile))
                {
                    await WriteString("You cannot ban other admins or the owner of the server.", "text/plain", 400);
                    return null;
                }
            }

            //Build
            return new RpcCommand
            {
                opcode = -2,
                payload = new RpcData
                {
                    userid = s.steam_id
                },
                persist = true
            };
        }

        class RpcData
        {
            public string userid;
        }

        class RequestData
        {
            public string steam_id;
        }
    }
}
