using ArkSaveEditor.World.WorldTypes.ArkTribeLogEntries;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapMasterServer.NetEntities
{
    public class ArkHubReply
    {
        public ArkHubWildcardNews ark_news;
        public List<UsersMeReply_Server> servers;
    }

    public class ArkHubWildcardNews
    {
        public string title;
        public string img;
        public string link;
        public string content;
    }
}
