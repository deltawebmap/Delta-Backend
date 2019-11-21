using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Auth.OAuth
{
    public static class OAuthHttpHandler
    {
        public static async Task OnOAuthRequest(Microsoft.AspNetCore.Http.HttpContext e, string next)
        {
            var method = Program.FindRequestMethod(e);
            if (next == "/query" && method == RequestHttpMethod.post)
                await OAuthQuery.OnQueryRequest(e);
            else if (next == "/authorize" && method == RequestHttpMethod.get)
                await OAuthAuthorize.OnAuthorizeRequest(e);
            else if (next == "/verify" && method == RequestHttpMethod.post)
                await OAuthVerifyRequest.OnVerifyRequest(e);
            else
                throw new StandardError("Endpoint not found.", StandardErrorCode.NotFound);
        }
    }
}
