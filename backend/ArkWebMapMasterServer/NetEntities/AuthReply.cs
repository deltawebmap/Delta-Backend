using ArkWebMapMasterServer.PresistEntities;
using LibDeltaSystem.Db.System;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapMasterServer.NetEntities
{
    public class AuthReply
    {
        public bool ok;
        public string message;
        public DbUser user;
        public string token;
        public string next;
    }
}
