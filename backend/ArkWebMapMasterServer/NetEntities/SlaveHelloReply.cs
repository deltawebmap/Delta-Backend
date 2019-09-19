using ArkWebMapMasterServer.PresistEntities;
using LibDeltaSystem.Db.System;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapMasterServer.NetEntities
{
    public class SlaveHelloReply
    {
        public SlaveHelloReply_MessageType status;
        public Dictionary<string, string> status_info;

        public DbServer serverInfo;
    }

    public enum SlaveHelloReply_MessageType
    {
        Ok = 0,
        MasterOutOfDate = 1, //Master is a version behind the client somehow
        SlaveOutOfDate = 2, //Slave is too old.
        ServerDeleted = 3, //Server requested was deleted.
    }
}
