using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapMasterServer.PresistEntities
{
    public class SavedMapEntry
    {
        public int _id { get; set; }

        public string server_id { get; set; }
        public int tribe_id { get; set; }
        public int map_id { get; set; }

        public string map_name { get; set; }
    }
}
