using ArkHttpServer.NetEntities.ItemSearch;
using ArkSaveEditor;
using ArkSaveEditor.ArkEntries;
using ArkSaveEditor.World;
using ArkSaveEditor.World.WorldTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkHttpServer.HttpServices
{
    public static class TribeInventorySearchService
    {
        public static Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, ArkWorld world, int tribeId)
        {
            //Get query.
            string query = e.Request.Query["q"].ToString().ToLower();

            //Get page offset
            int page_offset = int.Parse(e.Request.Query["p"]);

            //Find all item entries with this name. We'll search for their classname in the inventories.
            var itemEntries = ArkImports.item_entries.Where(x => x.name.ToLower().Contains(query)).ToArray();

            //Now, search all tribe dinos and see if their inventory contains any of these classnames.
            Dictionary<string, WebArkInventoryItemResult> inventories = SearchDinoInventories(world, tribeId, itemEntries, out Dictionary<string, ArkDinosaur> dinos);

            //Mix the inventories down to a list
            List<WebArkInventoryItemResult> inventoriesList = new List<WebArkInventoryItemResult>();
            foreach (var i in inventories.Values)
                inventoriesList.Add(i);

            //Convert all of the dinos to a basic form
            Dictionary<string, WebArkInventoryDino> basicDinos = new Dictionary<string, WebArkInventoryDino>();
            foreach(var d in dinos)
            {
                ArkDinosaur di = d.Value;
                ArkDinoEntry entry = di.dino_entry;
                if(entry == null)
                {
                    basicDinos.Add(d.Key, new WebArkInventoryDino
                    {
                        id = di.dinosaurId.ToString(),
                        img = "https://ark.romanport.com/assets/img_failed.png",
                        displayClassName = di.classnameString,
                        displayName = di.tamedName,
                        level = di.level
                    });
                } else
                {
                    basicDinos.Add(d.Key, new WebArkInventoryDino
                    {
                        id = di.dinosaurId.ToString(),
                        img = entry.icon_url,
                        displayClassName = entry.screen_name,
                        displayName = di.tamedName,
                        level = di.level
                    });
                }
            }

            //Sort by whatever the closest is to the typed name
            var itemsReply = inventoriesList.ToArray();
            Array.Sort(itemsReply, delegate (WebArkInventoryItemResult x, WebArkInventoryItemResult y) {
                return x.item_displayname.Length.CompareTo(y.item_displayname.Length);
            });

            //Now, create a reply.
            WebArkInventoryItemReply reply = new WebArkInventoryItemReply
            {
                items = itemsReply,
                more = false,
                owner_inventory_dino = basicDinos,
                query = query
            };

            //Write the reply
            return ArkWebServer.QuickWriteJsonToDoc(e, reply);
        }

        static Dictionary<string, WebArkInventoryItemResult> SearchDinoInventories(ArkWorld w, int tribeId, ArkItemEntry[] searchItemEntries, out Dictionary<string, ArkDinosaur> dinos)
        {
            //First, find tribe dinos
            var tribeDinos = w.dinos.Where(x => x.tribeId == tribeId);

            //Create the dict of items
            Dictionary<string, WebArkInventoryItemResult> results = new Dictionary<string, WebArkInventoryItemResult>();
            dinos = new Dictionary<string, ArkDinosaur>();

            //Now, find all dinos with this item in their inventory
            foreach (var d in tribeDinos)
            {
                var inventory = d.GetInventoryItems(false);

                //Loop through inventory items and see if it matches any of the items we are searching for.
                foreach(var item in inventory)
                {
                    var itemEntryResults = searchItemEntries.Where(x => x.classname == item.classnameString).ToArray();
                    if (itemEntryResults.Length != 1)
                        continue;
                    ArkItemEntry entry = itemEntryResults[0];

                    //Match. Add the item to the results.
                    if(!results.ContainsKey(entry.classname))
                    {
                        results.Add(entry.classname, new WebArkInventoryItemResult
                        {
                            item_classname = entry.classname,
                            item_displayname = entry.name,
                            item_icon = entry.icon.icon_url,
                            owner_inventories = new List<WebArkInventoryItemResultInventory>
                            {
                                new WebArkInventoryItemResultInventory
                                {
                                    id = d.dinosaurId.ToString(),
                                    type = WebArkInventoryItemResultInventoryType.Dino,
                                    count = item.stackSize
                                }
                            },
                            total_count = item.stackSize
                        });
                    } else
                    {
                        //Add to the total and add ourselves to the dino inventory if we're not already there
                        if (results[entry.classname].owner_inventories.Where( x => x.id == d.dinosaurId.ToString()).Count() == 0)
                        {
                            //Does not exist.
                            results[entry.classname].owner_inventories.Add(new WebArkInventoryItemResultInventory
                            {
                                count = item.stackSize,
                                id = d.dinosaurId.ToString(),
                                type = WebArkInventoryItemResultInventoryType.Dino
                            });
                        } else
                        {
                            //Does exist
                            results[entry.classname].owner_inventories.Where(x => x.id == d.dinosaurId.ToString()).First().count += item.stackSize;
                        }
                            
                        results[entry.classname].total_count += item.stackSize;
                    }

                    //Add this dino owner if it hasn't yet been added
                    if (!dinos.ContainsKey(d.dinosaurId.ToString()))
                        dinos.Add(d.dinosaurId.ToString(), d);
                }
            }

            return results;
        }
    }
}
