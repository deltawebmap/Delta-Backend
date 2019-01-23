using ArkSaveEditor.World;
using ArkSaveEditor.World.WorldTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ArkHttpServer.Entities
{
    public class HttpSession
    {
        public ArkWorld world;
        public string session_id;
        public string tribe_name;
        public string game_file_path;
        public List<HttpSessionEvent> new_events = new List<HttpSessionEvent>();
        public Dictionary<string, ArkItemSearchResultsItem> item_dict_cache = new Dictionary<string, ArkItemSearchResultsItem>(); //Key: Item classname

        public byte[] last_file_hash; //Used to tell when the file is updated
        public DateTime last_heartbeat_time; //Updated when a request comes in.
        public List<string> last_dino_list;

        public byte[] GetComputedFileHash()
        {
            byte[] output;
            try
            {
                using (FileStream fs = new FileStream(game_file_path, FileMode.Open))
                {
                    using (SHA256 s = SHA256.Create())
                        output = s.ComputeHash(fs);
                }
            } catch
            {
                output = new byte[0];
            }
            return output;
        }

        public bool CompareExistingHashWithNewHash(byte[] newHash)
        {
            //Compare each byte
            if (newHash.Length != last_file_hash.Length)
                return false;

            for(int i = 0; i<newHash.Length; i++)
            {
                if (newHash[i] != last_file_hash[i])
                    return false;
            }

            return true;
        }

        public void RecomputeItemDictCache(List<ArkDinosaur> dinos)
        {
            lock(item_dict_cache)
            {
                item_dict_cache.Clear();
                foreach(var d in dinos)
                {
                    //Add tuple with inventory item and dino ID
                    string dinoId = d.dinosaurId.ToString();
                    List<ArkPrimalItem> dino_inventory;
                    try
                    {
                        dino_inventory = d.GetInventoryItems();
                    } catch
                    {
                        //Skip dino.
                        continue;
                    }

                    //There will be duplicate values in this, but that is OK.
                    foreach(var i in dino_inventory)
                    {
                        string classname = i.classnameString;

                        //If the item dict does not contain this item, also add an entry
                        if (!item_dict_cache.ContainsKey(classname))
                        {
                            item_dict_cache.Add(classname, new ArkItemSearchResultsItem
                            {
                                classname = classname,
                                entry = ArkSaveEditor.ArkImports.GetItemDataByClassname(classname),
                                owner_ids = new Dictionary<string, int>(),
                                total_count = 0
                            });
                        }

                        //Set values in the dict
                        item_dict_cache[classname].total_count += i.stackSize;
                        if (!item_dict_cache[classname].owner_ids.ContainsKey(dinoId))
                        {
                            //Does not contain this dino. Add it
                            item_dict_cache[classname].owner_ids.Add(dinoId, i.stackSize);

                            //Also add this dino's data
                            item_dict_cache[classname].owner_dinos.Add(dinoId, new BasicArkDino(d, world, session_id));
                        }
                        else
                            //Does contain our dino. Add this stack
                            item_dict_cache[classname].owner_ids[dinoId] += i.stackSize;
                    }
                }
            }
        }
    }

    public class HttpSessionEvent
    {
        public object data;
        public HttpSessionEventType type;
        public DateTime time;

        public HttpSessionEvent(object data, HttpSessionEventType type)
        {
            this.data = data;
            this.type = type;
            time = DateTime.UtcNow;
        }
    }

    public enum HttpSessionEventType
    {
        MapUpdate = 0, //Issued when the Ark map file is updated.
    }
}
