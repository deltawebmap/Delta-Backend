using LibDeltaSystem.Db.System;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Machines
{
    public static class CreateMachineRequest
    {
        public static async Task OnUserCreateMachine(Microsoft.AspNetCore.Http.HttpContext e, DbUser user)
        {
            //Decode POST body
            var body = Program.DecodePostBody<CreateMachineRequestPayload>(e);

            //Add the machine
            var machine = await DbMachine.CreateMachineAsync(Program.connection, "USER", user._id, body.name);

            //Create response
            CreateMachineRequestResponse response = new CreateMachineRequestResponse
            {
                name = machine.name,
                token = machine.token
            };

            //Send
            await Program.QuickWriteJsonToDoc(e, response);
        }

        public class CreateMachineRequestPayload
        {
            /// <summary>
            /// Title of the machine. Doesn't mean much.
            /// </summary>
            public string name;
        }

        public class CreateMachineRequestResponse
        {
            /// <summary>
            /// Title of the machine. Doesn't mean much.
            /// </summary>
            public string name;

            /// <summary>
            /// The token to use
            /// </summary>
            public string token;
        }
    }
}
