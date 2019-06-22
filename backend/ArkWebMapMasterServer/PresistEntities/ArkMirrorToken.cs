using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapMasterServer.PresistEntities
{
    public class ArkMirrorToken
    {
        public string _id { get; set; }
        public string token { get; set; }
        public bool hasToken { get; set; }
        public Dictionary<string, string> dinoTokenMap { get; set; }
        public long time { get; set; }
    }
}
