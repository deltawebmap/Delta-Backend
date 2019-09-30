using LibDeltaSystem.Db.System;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Machines
{
    public static class MachinesHttpHandler
    {
        public static async Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, string path)
        {
            //Get method
            var method = Program.FindRequestMethod(e);

            //We aren't creating one. Authenticate these machines using their token.
            string token = e.Request.Headers["X-Machine-Token"];
            DbMachine machine = await Program.connection.AuthenticateMachineTokenAsync(token);
            if(machine == null)
            {
                await Program.QuickWriteToDoc(e, "Machine Authentication Failed. Check the X-Machine-Token HTTP header.", "text/plain", 401);
                return;
            }

            //Now, handle as usual.
            if (path == "create_server" && method == RequestHttpMethod.post)
                await MachineCreateServerRequest.OnHttpRequest(e, machine);
            else if (path == "info" && method == RequestHttpMethod.get)
                await MachineQueryInfo.OnHttpRequest(e, machine);
            else if (path == "files" && method == RequestHttpMethod.post)
                await MachineFileListRequest.OnHttpRequest(e, machine);
            else if (path == "file_callback" && method == RequestHttpMethod.post)
                await MachineFileListRequest.OnCallbackHttpRequest(e, machine);
            else if (path == "activate" && method == RequestHttpMethod.post)
                await MachineActivateRequest.OnActivateRequest(e, machine);
            else if (path == "await_activation" && method == RequestHttpMethod.get)
                await MachineActivateRequest.OnWaitForActivationRequest(e, machine);
            else
                await Program.QuickWriteToDoc(e, "Endpoint Not Found", "text/plain", 404);
        }
    }
}
