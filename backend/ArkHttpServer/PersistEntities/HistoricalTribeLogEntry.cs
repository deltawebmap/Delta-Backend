using System;
using System.Collections.Generic;
using System.Text;

namespace ArkHttpServer.PersistEntities
{
    public class HistoricalTribeLogEntry
    {
        public int _id { get; set; } //Tribe ID
        public string content { get; set; }
        public long time { get; set; }
    }
}
