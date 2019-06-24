using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapLightspeed
{
    public class PendingRequest
    {
        public Microsoft.AspNetCore.Http.HttpContext e;
        public Task awaiter;
        public int token;
    }
}
