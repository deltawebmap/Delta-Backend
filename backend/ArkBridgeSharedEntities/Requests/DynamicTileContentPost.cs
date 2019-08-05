using ArkSaveEditor.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkBridgeSharedEntities.Requests
{
    public class DynamicTileContentPost
    {
        public string server_id { get; set; }
        public string server_creds { get; set; }//Base-64 encoded

        public int version { get; set; } //Data version

        public Dictionary<string, string> tokens { get; set; } //Upload tokens {type, token}

        public long time { get; set; }
    }
}
