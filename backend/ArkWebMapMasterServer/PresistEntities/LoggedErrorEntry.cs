using ArkBridgeSharedEntities.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapMasterServer.PresistEntities
{
    public class LoggedErrorEntry
    {
        public int _id { get; set; }
        public string description { get; set; }
        public bool isStandardError { get; set; }
        public StandardErrorCode standardErrorCode { get; set; }
        public string stackTrace { get; set; }
        public long time { get; set; }
        public bool isAuth { get; set; }
        public string endpoint { get; set; }
        public string method { get; set; }
        public string authUserId { get; set; }
    }
}
