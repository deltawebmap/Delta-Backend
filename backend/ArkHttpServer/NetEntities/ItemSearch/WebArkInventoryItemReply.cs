using ArkHttpServer.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkHttpServer.NetEntities.ItemSearch
{
    public class WebArkInventoryItemReply
    {
        public List<WebArkInventoryItemResult> items;
        public bool more; //Do more exist?
        public string query;
        public int page_offset;
        public int total_item_count; //Total inventory count, even if it isn't sent on this page.
        public Dictionary<int, Dictionary<string, WebArkInventoryHolder>> inventories;
    }
}
