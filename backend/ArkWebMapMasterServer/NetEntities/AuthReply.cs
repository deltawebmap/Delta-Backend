using ArkWebMapMasterServer.PresistEntities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapMasterServer.NetEntities
{
    public class AuthReply
    {
        public bool ok;
        public string message;
        public ArkUser user;
        public string token;
        public string next;
    }
}
