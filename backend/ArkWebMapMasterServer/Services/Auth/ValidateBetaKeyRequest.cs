using LibDeltaSystem;
using LibDeltaSystem.WebFramework.ServiceTemplates;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
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
            //Get request data
            RequestData request = await ReadPOSTContentChecked<RequestData>();

            //Check
            bool ok = await conn.ValidateAndClaimBetaKey(request.key, ObjectId.Parse(request.user_id));

            //Return
            await WriteJSON(new ResponseData
            {
                key = request.key,
                ok = ok
            });
        }

        class RequestData
        {
            public string key;
            public string user_id;
        }

        class ResponseData
        {
            public string key;
            public bool ok;
        }
    }
}
