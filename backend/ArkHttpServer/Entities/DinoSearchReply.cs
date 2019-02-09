using ArkSaveEditor.ArkEntries;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkHttpServer.Entities
{
    public class DinoSearchReply
    {
        public List<ArkDinoEntry> results;
        public string query;
    }
}
