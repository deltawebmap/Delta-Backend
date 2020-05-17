using LibDeltaSystem;
using LibDeltaSystem.WebFramework.ServiceTemplates;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Auth
{
    public class ValidateBetaKeyRequest : BasicDeltaService
    {
        public ValidateBetaKeyRequest(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public override async Task OnRequest()
        {
            string key = e.Request.Query["beta_key"].ToString().ToUpper();
            bool ok = await conn.ValidateAndClaimBetaKey(key, null);
            await WriteJSON(new ResponseData
            {
                key = key,
                ok = ok
            });
        }

        class ResponseData
        {
            public string key;
            public bool ok;
        }
    }
}
