using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapMasterServer.NetEntities
{
    public class MapListRequestResponse
    {
        public MapListRequestResponseEntry[] maps;
    }

    public class MapListRequestResponseEntry
    {
        public string name;
        public int id;
        public string url;
    }
}
