using System;
using System.Collections.Generic;
using System.Text;

namespace ArkHttpServer.NetEntities.ItemSearch
{
    public class WebArkInventoryItemResult
    {
        public string item_classname;
        public string item_displayname;
        public string item_icon;
        public int total_count;
        public List<WebArkInventoryItemResultInventory> owner_inventories;
    }

    public class WebArkInventoryItemResultInventory
    {
        public string id;
        public WebArkInventoryItemResultInventoryType type;
        public int count;
    }

    public enum WebArkInventoryItemResultInventoryType
    {
        Dino = 0
    }
}
