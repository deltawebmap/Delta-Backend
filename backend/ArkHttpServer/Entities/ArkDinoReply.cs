using ArkSaveEditor;
using ArkSaveEditor.ArkEntries;
using ArkSaveEditor.Entities;
using ArkSaveEditor.Entities.LowLevel.DotArk;
using ArkSaveEditor.Entities.LowLevel.DotArk.ArkProperties;
using ArkSaveEditor.World;
using ArkSaveEditor.World.WorldTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkHttpServer.Entities
{
    public class ArkDinoReply
    {
        public ArkDinosaur dino;
        public List<ArkPrimalItem> inventory_items;
        public Dictionary<string, ArkItemEntry> item_class_data = new Dictionary<string, ArkItemEntry>();
        public ArkDinosaurStats max_stats;
        public ArkDinoEntry dino_entry;
        public string href;

        public ArkDinoReply(ArkDinosaur d, ArkWorld w)
        {
            //Set the dino data
            dino = d;

            //Get the various components
            try
            {
                inventory_items = d.GetInventoryItems(false);

                //If we got the inventory items, get the item data too.
                foreach (var i in inventory_items)
                {
                    string classname = i.classname;
                    if(!item_class_data.ContainsKey(classname))
                    {
                        item_class_data.Add(classname, ArkImports.GetItemDataByClassname(classname));
                    }
                }
            } catch
            {
                inventory_items = new List<ArkPrimalItem>();
            }

            //Get max stats
            max_stats = dino.GetMaxStats();

            //Set dino entry
            dino_entry = dino.dino_entry;

            //Set href
            href = $"{ArkWebServer.api_prefix}/world/dinos/{d.dinosaurId.ToString()}";
        }
    }
}
