using LibDeltaSystem;
using LibDeltaSystem.Db.System;
using LibDeltaSystem.Entities.CommonNet;
using LibDeltaSystem.WebFramework.ServiceTemplates;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Servers.Admin
{
    public class AdminServerSetNameRequest : ArkServerAdminDeltaService
    {
        public AdminServerSetNameRequest(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public override async Task OnAuthenticatedRequest()
        {
            //Decode
            RequestData request = await ReadPOSTContentChecked<RequestData>();
            if (request == null)
                return;

            //Validate
            if(request.name.Length < 2 || request.name.Length > 32)
            {
                await WriteString("Name must be between 2 and 32 characters in length.", "text/plain", 400);
                return;
            }

            //Set
            await server.UpdateServerNameAsync(conn, request.name);

            //Return server
            await WriteJSON(await NetGuildUser.GetNetGuild(conn, server, user));
        }

        class RequestData
        {
            public string name;
        }
    }
}
