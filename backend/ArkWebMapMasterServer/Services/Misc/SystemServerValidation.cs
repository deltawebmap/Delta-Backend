using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Misc
{
    public static class SystemServerValidation
    {
        public static Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e)
        {
            //From the URL, get the type and key
            string type = e.Request.Query["type"];
            string value = e.Request.Query["value"];

            //If the keys contain this, validate
            if(Program.config.system_server_keys.ContainsKey(type))
            {
                if(Program.config.system_server_keys[type] == value)
                {
                    return Program.QuickWriteStatusToDoc(e, true);
                }
            }
            return Program.QuickWriteStatusToDoc(e, false);
        }
    }
}
