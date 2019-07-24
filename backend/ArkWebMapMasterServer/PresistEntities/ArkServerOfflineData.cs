using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapMasterServer.PresistEntities
{
    public class ArkServerOfflineData
    {
        public string _id { get; set; } //In format {serverid}/{tribeid}
        public byte[] content { get; set; } //The actual data
        public long time { get; set; } //Time pushed
        public int version { get; set; } //Version of this
    }
}
