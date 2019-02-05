using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapMasterServer.NetEntities
{
    public class SteamProfile
    {
        public string steamid;
        public string profilestate;
        public string personaname;
        public string profileurl;
        public string avatarfull;
        public long timecreated;
    }

    public class SteamProfile_Players
    {
        public List<SteamProfile> players;
    }

    public class SteamProfile_Full
    {
        public SteamProfile_Players response;
    }
}
