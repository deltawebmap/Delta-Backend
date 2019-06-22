using System;
using System.Collections.Generic;
using System.Text;

namespace ArkHttpServer.Entities
{
    public class ArkWebMapMirrorTokens
    {
        public string token { get; set; }
        public Dictionary<string, string> dinoTokenMap { get; set; } //Maps dino tokens to dino IDs
        public long updateTime { get; set; }
    }
}
