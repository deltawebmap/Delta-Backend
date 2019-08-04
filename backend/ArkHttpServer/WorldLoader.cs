using ArkHttpServer.Entities;
using ArkSaveEditor;
using ArkSaveEditor.Deserializer.DotArk;
using ArkSaveEditor.World;
using ArkSaveEditor.World.WorldTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ArkHttpServer
{
    public delegate void OnMapReloadEvent(ArkWorld world, DateTime time);
    public static class WorldLoader
    {
        private static ArkWorld current_world;
        private static DateTime current_world_time;
        private static bool is_current_load_loaded = false;

        private static DateTime last_check_world_time; //Used by CheckForMapUpdates. Set to the time of the last check.
        private static bool has_last_checked_world_time = false;

        private static Dictionary<int, Dictionary<string, ArkItemSearchResultsItem>> item_dict_cache;
        private static bool item_dict_cache_created = false;

        private static string world_path
        {
            get
            {
                return ArkWebServer.config.save_location;
            }
        }

        //API
        public static ArkWorld GetWorld(out DateTime time)
        {
            //If the world is loaded, return it. Else, reload it
            time = DateTime.MinValue;
            if (is_current_load_loaded)
            {
                time = current_world_time;
                return current_world;
            }

            //Load new world
            LoadArkWorldIntoSlot();
            time = current_world_time;
            return current_world;
        }

        public static ArkWorld GetWorld()
        {
            return GetWorld(out DateTime time);
        }

        public static int GetTribeIdForSteamId(string steamId)
        {
            //Look up player
            var players = GetWorld().players.Where(x => x.steamPlayerId == steamId).ToArray();
            if (players.Length != 1)
                return -1;
            return players[0].tribeId;
        }

        public static Dictionary<string, ArkItemSearchResultsItem> GetItemDictForTribe(int tribeId)
        {
            return item_dict_cache[tribeId];
        }

        /// <summary>
        /// Checks if the map file has been updated. If it has, returns true. Else, returns false.
        /// </summary>
        /// <returns></returns>
        public static bool CheckForMapUpdates()
        {
            //If the map is not loaded, return true.
            if (!is_current_load_loaded)
                return true;

            //If we have never checked, check time and return false
            if(!has_last_checked_world_time)
            {
                last_check_world_time = GetLastWorldEditTime();
                has_last_checked_world_time = true;
                return false;
            }

            //Now, compare the last edit time of the current file to that of the old file.
            DateTime nowFileTime = GetLastWorldEditTime();
            bool status = nowFileTime.Ticks != last_check_world_time.Ticks;

            //Update the time
            last_check_world_time = nowFileTime;

            //If the map has been updated, update the memory version
            if(status)
            {
                lock(current_world)
                {
                    LoadArkWorldIntoSlot();
                }
            }

            //Return status
            return status;
        }

        //Private
        private static void LoadArkWorldIntoSlot()
        {
            DateTime world_time;
            ArkWorld world;

            //world = new ArkWorld(ArkWebServer.config.save_location, ArkWebServer.config.save_map, @"C:\Program Files (x86)\Steam\steamapps\common\ARK\ShooterGame\Saved\Config\WindowsServer\");

            //Try to access data
            int retry = 0;
            while(true)
            {
                try
                {
                    //Set the current world time
                    world_time = GetLastWorldEditTime();

                    //Get world
                    world = new ArkWorld(ArkWebServer.config.save_location, ArkWebServer.config.save_map, ArkWebServer.config.ark_config);

                    //Done
                    break;
                }
                catch (Exception ex)
                {
                    if(retry == 5)
                    {
                        //Failed
                        throw new Exception("Failed to load ARK map after 5 retries.");
                    } else
                    {
                        //Do retry
                        Console.WriteLine($"Failed to load ARK save file. Retry {retry + 1}/5. Retrying in 2 seconds...");
                        System.Threading.Thread.Sleep(2000);
                        retry++;
                    }
                }
            }

            //Compute item dict
            Dictionary<int, Dictionary<string, ArkItemSearchResultsItem>> itemDict = ComputeItemDictCache(world);

            //Gather info for the Ark Web Map Mirror plugin
            MirrorPlugin.OnMapSave(world);

            //Submit new tribe ID entries
            try
            {
                Tools.SubmitHubDataTool.SubmitHubData(world, world_time);
            } catch (Exception ex)
            {
                Console.WriteLine("Failed to submit tribe log entries with error " + ex.Message + ex.StackTrace);
            }

            //Set values
            current_world = world;
            item_dict_cache = itemDict;
            current_world_time = world_time;
            is_current_load_loaded = true;
        }

        /// <summary>
        /// Returns classname strings mapped to inventories
        /// </summary>
        /// <param name="world"></param>
        /// <param name="tribeId"></param>
        /// <returns></returns>
        public static Dictionary<int, Dictionary<string, ArkItemSearchResultsItem>> ComputeItemDictCache(ArkWorld world)
        {
            Dictionary<int, Dictionary<string, ArkItemSearchResultsItem>> masterItemDict = new Dictionary<int, Dictionary<string, ArkItemSearchResultsItem>>();

            //Find all characters of tribe
            List<ArkCharacter> characters = new List<ArkCharacter>();
            characters.AddRange(world.playerCharacters.Where(x => x.isInTribe));
            characters.AddRange(world.dinos.Where(x => x.isInTribe));

            foreach (var d in characters)
            {
                //Find (or create) item dict
                Dictionary<string, ArkItemSearchResultsItem> itemDict;
                if (masterItemDict.ContainsKey(d.tribeId))
                    itemDict = masterItemDict[d.tribeId];
                else
                {
                    itemDict = new Dictionary<string, ArkItemSearchResultsItem>();
                    masterItemDict.Add(d.tribeId, itemDict);
                }

                //Get inventory
                List<ArkPrimalItem> inventory;
                inventory = d.GetInventory().items;

                //Find type and ID
                ArkItemSearchResultsInventoryType type;
                string id;
                if (d.GetType() == typeof(ArkDinosaur))
                {
                    ArkDinosaur dino = (ArkDinosaur)d;
                    id = dino.dinosaurId.ToString();
                    type = ArkItemSearchResultsInventoryType.Dino;
                } else if (d.GetType() == typeof(ArkPlayer))
                {
                    ArkPlayer player = (ArkPlayer)d;
                    id = player.steamId;
                    type = ArkItemSearchResultsInventoryType.Player;
                    if (!player.isAlive)
                        continue;
                } else
                {
                    //Skip
                    continue;
                }

                //There will be duplicate values in this, but that is OK.
                foreach (var i in inventory)
                {
                    //Get item classname
                    string itemClassname = i.classnameString;

                    //Find or create item type
                    ArkItemSearchResultsItem item;
                    if (itemDict.ContainsKey(itemClassname))
                        item = itemDict[itemClassname];
                    else {
                        item = new ArkItemSearchResultsItem
                        {
                            classname = itemClassname,
                            owners = new List<ArkItemSearchResultsInventory>(),
                            total_count = 0
                        };
                        itemDict.Add(itemClassname, item);
                    }

                    //Add this to the inventory
                    item.total_count += i.stackSize;
                    var matchingParents = item.owners.Where(x => x.type == type && x.id == id);
                    if (matchingParents.Count() == 0)
                    {
                        item.owners.Add(new ArkItemSearchResultsInventory
                        {
                            id = id,
                            type = type,
                            count = i.stackSize,
                            character = d
                        });
                    } else
                    {
                        matchingParents.First().count += i.stackSize;
                    }
                }
            }
            return masterItemDict;
        }

        private static DateTime GetLastWorldEditTime()
        {
            return File.GetLastWriteTimeUtc(world_path);
        }
    }
}
