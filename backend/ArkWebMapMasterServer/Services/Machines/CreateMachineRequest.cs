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

            //Generate split shortcode
            string displayShortcode = "";
            int shortcodeSegments = machine.shorthand_token.Length / 4;
            for (int i = 0; i < shortcodeSegments; i++) {
                displayShortcode += machine.shorthand_token.Substring(i * 4, 4);
                if (i + 1 != shortcodeSegments)
                    displayShortcode += "-";
            }

            //Create response
            CreateMachineRequestResponse response = new CreateMachineRequestResponse
            {
                name = machine.name,
                token = machine.token,
                shorthand = displayShortcode
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

            /// <summary>
            /// Token for the user to type
            /// </summary>
            public string shorthand;
        }
    }
}
