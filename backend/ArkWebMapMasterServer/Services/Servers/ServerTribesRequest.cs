using ArkWebMapMasterServer.ServiceTemplates;
using LibDeltaSystem;
using LibDeltaSystem.Entities.CommonNet;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Servers
{
    public class ServerTribesRequest : MasterTribeServiceTemplate
    {
        public ServerTribesRequest(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public override async Task OnRequest()
        {
            //Get tribes
            var tribes = await server.GetAllTribesAsync(conn);

            //Convert all
            ResponseData response = new ResponseData
            {
                tribes = new List<NetTribe>()
            };
            foreach (var t in tribes)
                response.tribes.Add(NetTribe.ConvertTribe(t));

            //Write
            await WriteJSON(response);
        }

        class ResponseData
        {
            public List<NetTribe> tribes;
        }
    }
}
