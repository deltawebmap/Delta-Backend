using ArkWebMapMasterServer.NetEntities.Managers;
using ArkWebMapMasterServer.PresistEntities.Managers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Providers
{
    public static class ProvidersMeService
    {
        public static Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, ArkManager me)
        {
            ManagerMe o = ManagerMe.Generate(me);
            return Program.QuickWriteJsonToDoc(e, o);
        }
    }
}
