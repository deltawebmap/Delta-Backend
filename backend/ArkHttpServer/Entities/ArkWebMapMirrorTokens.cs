using System;
using System.Collections.Generic;
using System.Text;

namespace ArkHttpServer.Entities
{
    public class ArkWebMapMirrorTokens
    {
        public string token;
        public Dictionary<string, string> dinoTokenMap; //Maps dino tokens to dino IDs
        public long updateTime;
    }
}
