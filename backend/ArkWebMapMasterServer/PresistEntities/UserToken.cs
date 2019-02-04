using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapMasterServer.PresistEntities
{
    public class UserToken
    {
        public string token { get; set; }
        public string uid { get; set; }
        public long createTime { get; set; }
        public int _id { get; set; }
    }
}
