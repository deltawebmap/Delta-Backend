using ArkHttpServer.Entities;
using ArkHttpServer.NetEntities.ItemSearch;
using ArkSaveEditor;
using ArkSaveEditor.ArkEntries;
using ArkSaveEditor.World;
using ArkSaveEditor.World.WorldTypes;
using ArkWebMapLightspeedClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkHttpServer.HttpServices
{
    public static class TribeInventorySearchService
    {
        public const int PAGE_SIZE = 3;

        public static async Task OnHttpRequest(LightspeedRequest e, ArkWorld world, int tribeId)
        {
            //Get query.
            string query = e.query["q"].ToString().ToLower();

            //Get page offset
            int page_offset = 0;
            if (e.query.ContainsKey("p"))
            {
                if (!int.TryParse(e.query["p"], out page_offset))
                    throw new StandardError(StandardErrorType.InvalidArg, "Failed to parse page query as an integer.", "Check the 'p' parameter.");
                if(page_offset < 0)
                    throw new StandardError(StandardErrorType.InvalidArg, "Page offset cannot be less than 0.", "Check the 'p' parameter.");
            }

            //Get range
            int min = page_offset * PAGE_SIZE;
            int max = (page_offset + 1) * PAGE_SIZE;

            //Get item map for tribe
            Dictionary<string, ArkItemSearchResultsItem> map = WorldLoader.GetItemDictForTribe(tribeId);
            List<WebArkInventoryItemResult> inventoriesList = new List<WebArkInventoryItemResult>();
            Dictionary<int, Dictionary<string, WebArkInventoryHolder>> globalCharacters = new Dictionary<int, Dictionary<string, WebArkInventoryHolder>>(); //Characters we'll need to include, mapped by type.
            int total = 0;
            int index = 0;
            foreach (var i in map.Values)
            {
                //Try and get the entry
                var entry = ArkImports.GetItemDataByClassname(i.classname);
                if (entry == null)
                    continue; //Skip

                //Check if it even matches
                if (!entry.name.ToLower().Contains(query))
                    continue;

                //Add
                inventoriesList.Add(new WebArkInventoryItemResult
                {
                    item_classname = i.classname,
                    item_displayname = entry.name,
                    item_icon = entry.icon.image_thumb_url,
                    total_count = i.total_count,
                    owner_inventories = i.owners
                });
                total += i.total_count;

                //Add to global table
                foreach(var o in i.owners)
                {
                    //Convert type
                    int type = (int)o.type;
                    
                    //Add if doesn't exist
                    if (!globalCharacters.ContainsKey(type))
                        globalCharacters.Add(type, new Dictionary<string, WebArkInventoryHolder>());

                    //Check if the ID exists
                    if (!globalCharacters[type].ContainsKey(o.id))
                    {
                        //Convert to WebArkInventoryHolder
                        WebArkInventoryHolder holder;
                        if (o.character.GetType() == typeof(ArkDinosaur))
                            holder = WebArkInventoryDino.Convert((ArkDinosaur)o.character);
                        else if (o.character.GetType() == typeof(ArkPlayer))
                            holder = WebArkInventoryPlayer.Convert((ArkPlayer)o.character);
                        else
                            throw new Exception("Unexpected type.");

                        //Add
                        globalCharacters[type].Add(o.id, holder);
                    }
                }
            }

            //Create response
            WebArkInventoryItemReply response = new WebArkInventoryItemReply
            {
                inventories = globalCharacters,
                items = inventoriesList,
                more = false,
                page_offset = 0,
                query = query,
                total_item_count = total
            };

            //Write
            await e.DoRespondJson(response);
        }
    }
}
