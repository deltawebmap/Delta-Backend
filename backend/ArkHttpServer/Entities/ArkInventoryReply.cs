using ArkSaveEditor;
using ArkSaveEditor.ArkEntries;
using ArkSaveEditor.World.WorldTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkHttpServer.Entities
{
    public class ArkInventoryReply
    {
        public List<ArkPrimalItem> inventory_items;
        public Dictionary<string, ArkItemEntry> item_class_data = new Dictionary<string, ArkItemEntry>();

        public static ArkInventoryReply CreateInventory(ArkInventory inventory)
        {
            ArkInventoryReply r = new ArkInventoryReply();
            try
            {
                r.inventory_items = inventory.items;

                //If we got the inventory items, get the item data too.
                foreach (var i in r.inventory_items)
                {
                    string classname = i.classname;
                    if (!r.item_class_data.ContainsKey(classname))
                    {
                        r.item_class_data.Add(classname, ArkImports.GetItemDataByClassname(classname));
                    }
                }
            }
            catch
            {
                r.inventory_items = new List<ArkPrimalItem>();
            }
            return r;
        }
    }
}
