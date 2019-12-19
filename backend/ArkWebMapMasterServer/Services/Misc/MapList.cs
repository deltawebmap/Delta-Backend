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
            //Convert map list to our special format
            MapListResponse response = new MapListResponse();
            foreach (var m in Program.ark_maps)
                response.maps.Add(new MapListResponseObject
                {
                    name = m.Value.displayName,
                    background = m.Value.maps[0].url.Replace("{x}", "1").Replace("{y}", "1").Replace("{z}", "2")
                });
            await Program.QuickWriteJsonToDoc(e, response);
        }

        class MapListResponse
        {
            public List<MapListResponseObject> maps = new List<MapListResponseObject>();
        }

        class MapListResponseObject
        {
            public string name;
            public string background;
        }
    }
}
