using ArkBridgeSharedEntities.Entities;
using ArkWebMapMasterServer.Managers;
using ArkWebMapMasterServer.PresistEntities.Managers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Providers.InternalApi
{
    public static class InternalProvidersApiHandler
    {
        public static Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, string path)
        {
            //Authenticate user
            ArkManager user = null;
            if (e.Request.Headers.ContainsKey("X-Api-Token"))
            {
                //Read Authorization header
                string token = e.Request.Headers["X-Api-Token"];
                user = ManageAuth.GetManagersCollection().FindOne(x => x.api_token == token);
            }
            if (user == null)
                throw new StandardError("API key invalid or missing.", StandardErrorCode.AuthRequired);

            //Authenticate machine ID
            ArkManagerMachine machine = null;
            if (e.Request.Headers.ContainsKey("X-Machine-ID"))
            {
                //Read Authorization header
                string id = e.Request.Headers["X-Machine-ID"];
                machine = ManageMachines.GetMachineById(user, id);
            }
            if (machine == null)
                throw new StandardError("Machine ID missing, invalid, or for the wrong user.", StandardErrorCode.AuthRequired);

            //Handle
            if (path == "machine_config")
                return MachineConfigServiceHandler.OnHttpRequest(e, user, machine);

            throw new StandardError("Not Found", StandardErrorCode.NotFound);
        }
    }
}
