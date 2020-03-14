
using ArkWebMapMasterServer.NetEntities;
using LibDeltaSystem;
using LibDeltaSystem.Db.System;
using LibDeltaSystem.WebFramework.ServiceTemplates;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Users
{
    public class TokenDevalidateService : UserAuthDeltaService
    {
        public TokenDevalidateService(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public string tokenType;

        public override async Task OnRequest()
        {
            throw new NotImplementedException();
        }

        public override async Task<bool> SetArgs(Dictionary<string, string> args)
        {
            tokenType = args["TOKEN_TYPE"];
            return true;
        }
    }
}
