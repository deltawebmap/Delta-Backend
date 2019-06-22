using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Mirror
{
    public static class MirrorErrorService
    {
        public static Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e)
        {
            string msg = new StreamReader(e.Request.Body).ReadToEnd();

            //Return OK, but Ark won't care
            return Program.QuickWriteStatusToDoc(e, true);
        }
    }
}
