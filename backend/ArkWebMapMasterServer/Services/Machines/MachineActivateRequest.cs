using ArkBridgeSharedEntities.Entities;
using ArkBridgeSharedEntities.Entities.NewSubserver;
using LibDeltaSystem.Db.System;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Machines
{
    /// <summary>
    /// Used to handle first-activation of a machine.
    /// </summary>
    public static class MachineActivateRequest
    {
        public static async Task OnActivateRequest(Microsoft.AspNetCore.Http.HttpContext e)
        {
            //Decode POST
            MachineActivationPayload payload = Program.DecodePostBody<MachineActivationPayload>(e);

            //Get machine
            DbMachine machine = await Program.connection.GetMachineByShorthandTokenAsync(payload.shorthand_token);
            if(machine == null)
            {
                //Return error
                await Program.QuickWriteJsonToDoc(e, new ActivationResponse
                {
                    token = null,
                    ok = false
                });
                return;
            }

            //Set data
            machine.latest_activation_time = DateTime.UtcNow;
            machine.last_version_major = payload.version_major;
            machine.last_version_minor = payload.version_minor;
            machine.first_activation_time = DateTime.UtcNow;
            machine.is_activated = true;
            machine.shorthand_token = null;

            //Save
            await machine.UpdateAsync();

            //Return OK
            await Program.QuickWriteJsonToDoc(e, new ActivationResponse
            {
                token = machine.token,
                ok = true
            });
        }

        /// <summary>
        /// Waits for activation.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="machine"></param>
        /// <returns></returns>
        public static async Task OnWaitForActivationRequest(Microsoft.AspNetCore.Http.HttpContext e, DbMachine machine)
        {
            //Set the activation timeout
            if(!machine.is_activated)
            {
                machine.first_activation_time = DateTime.UtcNow;
                await machine.UpdateAsync();
            }
            
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
                await Task.Delay(2000);
            }

            //Timed out.
            await Program.QuickWriteStatusToDoc(e, false);
        }

        class ActivationResponse
        {
            public bool ok;
            public string token;
        }
    }
}
