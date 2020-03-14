using LibDeltaSystem;
using LibDeltaSystem.Entities.ArkEntries;
using LibDeltaSystem.WebFramework;
using LibDeltaSystem.WebFramework.ServiceTemplates;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Misc
{
    public class MapListRequest : BasicDeltaService
    {
        public MapListRequest(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public override async Task OnRequest()
        {
            var maps = await Program.connection.GetARKMaps();
            await Program.QuickWriteJsonToDoc(e, new MapListResponse
            {
                maps = maps
            });
        }

        class MapListResponse
        {
            public Dictionary<string, ArkMapEntry> maps;
        }
    }
}
