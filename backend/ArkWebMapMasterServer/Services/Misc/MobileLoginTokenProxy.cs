
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Misc
{
    /// <summary>
    /// Handles the mobile auth proxy. 9/8/2019
    /// 
    /// The application flow goes as follows
    /// [App] -> POST here
    /// [Server] -> Returns a token that the app will use
    /// [App] -> Redirect user to login screen
    /// ---===---
    /// (User logs in)
    /// [Login Page] -> Sends a PUT here with the token used
    /// ---===---
    /// [App] -> Sends a GET here and returns if the user has completed auth yet
    /// [App] -> Sends a DELETE here to clear it now that we've logged in
    /// </summary>
    public static class MobileLoginTokenProxy
    {
        private static Dictionary<string, string> responses = new Dictionary<string, string>();

        public static async Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e)
        {
            //Get the method
            var method = Program.FindRequestMethod(e);

            //If this is a POST, generate a unique code
            string code;
            if(method == RequestHttpMethod.post)
            {
                //Generate a code
                code = Program.GenerateRandomString(32);
                while (responses.ContainsKey(code))
                    code = Program.GenerateRandomString(32);

                //Add to codes
                responses.Add(code, null);

                //Send response
                await SendResponseCode(e, code);
                return;
            }

            //This is to a code. Get the code from the URL
            code = e.Request.Query["code"];

            //Check if this is a valid code
            if (!responses.ContainsKey(code))
                throw new StandardError("Code not found.", StandardErrorCode.NotFound);

            //Change what happens depending on the method
            if(method == RequestHttpMethod.get)
            {
                await SendResponseCode(e, code);
                return;
            } else if (method == RequestHttpMethod.put)
            {
                //Set data to what is in the POST body
                responses[code] = Program.GetPostBodyString(e);
                await SendResponseCode(e, code);
                return;
            } else if (method == RequestHttpMethod.delete)
            {
                //Delete
                responses.Remove(code);
                await Program.QuickWriteStatusToDoc(e, true);
                return;
            } else
            {
                throw new StandardError("Unexpected method.", StandardErrorCode.BadMethod);
            }
        }

        private static async Task SendResponseCode(Microsoft.AspNetCore.Http.HttpContext e, string code)
        {
            string data = responses[code];
            await Program.QuickWriteJsonToDoc(e, new ResponseData
            {
                code = code,
                complete = data != null,
                data = data
            });
        }

        private class ResponseData
        {
            public string code;
            public string data;
            public bool complete;
        }
    }
}
