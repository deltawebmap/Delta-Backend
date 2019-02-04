using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapMasterServer.PresistEntities
{
    public class AuthMethod_UsernamePassword : IAuthMethod
    {
        public byte[] salt { get; set; }
        public byte[] password { get; set; }
    }
}
