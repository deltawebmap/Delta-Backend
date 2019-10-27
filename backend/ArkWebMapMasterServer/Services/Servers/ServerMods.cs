using LibDeltaSystem.Db.System;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Servers
{
    public static class ServerMods
    {
        public static async Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, DbServer s)
        {
            //Find all mods and create a dict
            Dictionary<string, DbSteamModCache> mods = new Dictionary<string, DbSteamModCache>();
            foreach (var m in s.mods)
                mods.Add(m, await Program.connection.GetSteamModById(m));

            //Create response
            ResponseData response = new ResponseData
            {
                mods = s.mods,
                mod_data = mods
            };

            //Write response
            await Program.QuickWriteJsonToDoc(e, response);
        }

        class ResponseData
        {
            public Dictionary<string, DbSteamModCache> mod_data;
            public string[] mods;
            public List<string> total_supported_mods = new List<string>
            {
                /* Place mods marked as supported here */
            };
        }
    }
}
