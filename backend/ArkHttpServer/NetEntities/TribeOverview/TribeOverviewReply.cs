using ArkHttpServer.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkHttpServer.NetEntities.TribeOverview
{
    public class TribeOverviewReply
    {
        public List<TribeOverviewPlayer> tribemates;
        public List<TribeOverviewDino> dinos;
        public List<ArkDinoReply> baby_dinos;
        public string tribeName;
    }
}
