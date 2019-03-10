using System;
using System.Collections.Generic;
using System.Text;

namespace ArkBridgeSharedEntities.Entities
{
    public class SteamProfile
    {
        public string steamid { get; set; }
        public string profilestate { get; set; }
        public string personaname { get; set; }
        public string profileurl { get; set; }
        public string avatarfull { get; set; }
        public long timecreated { get; set; }
    }
}
