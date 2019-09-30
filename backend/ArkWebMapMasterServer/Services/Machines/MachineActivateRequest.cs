using ArkBridgeSharedEntities.Entities.NewSubserver;
using LibDeltaSystem.Db.System;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Machines
{
    public static class MachineActivateRequest
    {
        public static async Task OnActivateRequest(Microsoft.AspNetCore.Http.HttpContext e, DbMachine machine)
        {
            //Decode POST
            MachineActivationPayload payload = Program.DecodePostBody<MachineActivationPayload>(e);

            //Set data
            machine.latest_activation_time = DateTime.UtcNow;
            machine.last_version_major = payload.version_major;
            machine.last_version_minor = payload.version_minor;
            if (machine.is_activated)
                machine.first_activation_time = DateTime.UtcNow;
            machine.is_activated = true;

            //Save
            await machine.UpdateAsync();

            //Return OK
            await Program.QuickWriteStatusToDoc(e, true);
        }

        /// <summary>
        /// Waits for activation.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="machine"></param>
        /// <returns></returns>
        public static async Task OnWaitForActivationRequest(Microsoft.AspNetCore.Http.HttpContext e, DbMachine machine)
        {
            //Wait for us to time out or the server to be activated
            for(int i = 0; i<20; i++)
            {
                //Check
                machine = await Program.connection.GetMachineByIdAsync(machine.id);

                //If activated, return status
                if(machine.is_activated)
                {
                    await Program.QuickWriteStatusToDoc(e, true);
                    return;
                }

                //Wait
                await Task.Delay(1000);
            }

            //Timed out.
            await Program.QuickWriteStatusToDoc(e, false);
        }
    }
}
