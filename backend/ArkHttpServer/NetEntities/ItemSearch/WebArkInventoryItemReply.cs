using System;
using System.Collections.Generic;
using System.Text;

namespace ArkHttpServer.NetEntities.ItemSearch
{
    public class WebArkInventoryItemReply
    {
        public WebArkInventoryItemResult[] items;
        public bool more; //Do more exist?
        public Dictionary<string, WebArkInventoryDino> owner_inventory_dino;
        public string query;
        public int page_offset;
        public int total_item_count; //Total inventory count, even if it isn't sent on this page.
    }
}
