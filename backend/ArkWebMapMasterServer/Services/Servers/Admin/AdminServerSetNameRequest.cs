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

            //Update
            await server.GetUpdateBuilder(conn)
                .UpdateServerNameValidated(request.name)
                .Apply();

            //Return server
            await WriteJSON(await NetGuildUser.GetNetGuild(conn, server, user));
        }

        class RequestData
        {
            public string name;
        }
    }
}
