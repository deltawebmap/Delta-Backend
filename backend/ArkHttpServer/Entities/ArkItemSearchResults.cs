using ArkSaveEditor.ArkEntries;
using ArkSaveEditor.World.WorldTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkHttpServer.Entities
{
    public class ArkItemSearchResultsItem
    {
        public string classname;
        
        public ArkItemEntry entry;
        public int total_count;
        public Dictionary<string, int> owner_ids = new Dictionary<string, int>();

        public Dictionary<string, BasicArkDino> owner_dinos = new Dictionary<string, BasicArkDino>();
    }

    public class ArkItemSearchResults
    {
        public bool moreListItems; //Limit at 100
        public int hardLimit; //Limit

        public int itemEntriesFound;
        public int tribeItemsFound;

        public string query;

        public List<ArkItemSearchResultsItem> results;
    }
}
