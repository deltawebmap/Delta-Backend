using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkHttpServer.Entities
{
    public class DinoServerData
    {
        //Internal
        [JsonIgnore]
        public int _id { get; set; }
        [JsonIgnore]
        public int tribeId { get; set; }
        [JsonIgnore]
        public string dinoId { get; set; }

        public int dinoGroup { get; set; } //0: no group
    }
}
