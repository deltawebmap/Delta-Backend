using LibDeltaSystem.Entities.ArkEntries;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Misc
{
    public static class MapList
    {
        public static async Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e)
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
