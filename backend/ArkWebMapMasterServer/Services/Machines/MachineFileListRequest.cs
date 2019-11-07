using LibDeltaSystem.Db.System;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Machines
{
    /// <summary>
    /// Used to query the filesystem of machines for the in-browser filepicker
    /// </summary>
    public static class MachineFileListRequest
    {
        private static Dictionary<string, Stream> responses = new Dictionary<string, Stream>();

        public static async Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, DbMachine machine)
        {
            //Generate a token to use
            string token = Program.GenerateRandomString(16);
            while(responses.ContainsKey(token))
                token = Program.GenerateRandomString(16);
            responses.Add(token, null);

            //Read the POST body
            var body = Program.DecodePostBody<MachineFileListRequestBody>(e);

            //Create a Gateway message to send
            Program.gateway.SendMessageToSubserverWithId(new ArkWebMapGatewayClient.Messages.SubserverClient.MessageDirListing
            {
                opcode = ArkWebMapGatewayClient.GatewayMessageOpcode.MessageDirListing,
                headers = new Dictionary<string, string>(),
                pathname = body.path,
                token = token,
                callback_url = Program.config.endpoint_master+"/machines/file_callback"
            }, machine.id);

            //Wait for the response
            while (responses[token] == null)
                await Task.Delay(10);

            //Copy stream
            e.Response.ContentType = "application/json";
            await responses[token].CopyToAsync(e.Response.Body);

            //Close
            responses[token].Close();
            responses[token].Dispose();
            responses.Remove(token);
        }

        class MachineFileListRequestBody
        {
            public string path;
        }

        public static async Task OnCallbackHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, DbMachine machine)
        {
            //Read the token from the headers
            string token = e.Request.Headers["X-FileList-Token"];

            //If this token is invalid, stop
            if(!responses.ContainsKey(token))
            {
                await Program.QuickWriteStatusToDoc(e, false);
                return;
            }
            if (responses[token] != null)
            {
                await Program.QuickWriteStatusToDoc(e, false);
                return;
            }

            //Set the response
            MemoryStream ms = new MemoryStream();
            await e.Request.Body.CopyToAsync(ms);
            ms.Position = 0;
            responses[token] = ms;

            //Return ok
            await Program.QuickWriteStatusToDoc(e, true);
        }
    }
}
