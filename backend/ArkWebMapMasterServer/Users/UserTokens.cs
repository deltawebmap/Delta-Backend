using LibDeltaSystem.Db.System;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArkWebMapMasterServer.Users
{
    public static class UserTokens
    {
        public static string GenerateUserToken(DbUser u)
        {
            return u.MakeToken().GetAwaiter().GetResult();
        }

        public static DbUser ValidateUserToken(string token)
        {
            return Program.connection.AuthenticateUserToken(token).GetAwaiter().GetResult();
        }
    }
}
