using ArkHttpServer.Entities;
using ArkSaveEditor.World;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using ArkHttpServer.HttpServices;
using ArkSaveEditor.Entities;
using ArkSaveEditor.ArkEntries;
using ArkSaveEditor;
using System.IO;

namespace ArkHttpServer
{
    partial class ArkWebServer
    {
        public static Dictionary<string, HttpSession> sessions = new Dictionary<string, HttpSession>();

        public const int TRIBE_ITEMS_MAX_PAGE_RESULTS = 10;

        public const int CURRENT_CLIENT_VERSION = 1;

        public static Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, MasterServerArkUser user)
        {
            if (e.Request.Query.ContainsKey("v"))
            {
                Console.WriteLine($"Got net request to '{e.Request.Path}' on client version {e.Request.Query["v"].ToString()}.");
            } else
            {
                Console.WriteLine($"Got net request to '{e.Request.Path}'.");
            }
            try
            {
                //Set some headers
                e.Response.Headers.Add("Access-Control-Allow-Origin", "*");

                //Since we don't have to worry about permissions or anything, we'll just have a list of services created at compile time.
                string pathname = e.Request.Path.ToString().ToLower().Substring("/api".Length);

                //Handle things that are not map specific
                if (pathname.StartsWith("/create_session"))
                {
                    //Create a new session. For now, just use the same map.
                    return OnCreateSessionRequest(e, user);
                }
                if (pathname.StartsWith("/dino_search/"))
                {
                    //Search for dinos in the dino entry table.
                    string query = e.Request.Query["query"].ToString().ToLower();
                    List<ArkDinoEntry> dinos = new List<ArkDinoEntry>();
                    foreach (var d in ArkImports.dino_entries)
                    {
                        if (d.screen_name.ToLower().Contains(query))
                            dinos.Add(d);
                    }
                    return ArkWebServer.QuickWriteJsonToDoc(e, dinos);
                }

                //Continue to map specific things
                if (pathname.StartsWith("/world/"))
                {
                    //Get map name
                    string sessionId = pathname.Split('/')[2];

                    //Validate session ID
                    if (!sessions.ContainsKey(sessionId))
                        return ArkWebServer.QuickWriteToDoc(e, "Invalid session ID", "text/plain", 403);

                    //Trim session ID
                    pathname = pathname.Substring("/world/".Length + sessionId.Length);

                    //Get session
                    HttpSession session = sessions[sessionId];
                    session.latest_user = user;
                    ArkWorld world = session.world;

                    //Update heartbeat
                    session.last_heartbeat_time = DateTime.UtcNow;

                    if (pathname.StartsWith("/map/tiles/population/"))
                    {
                        return PopulationService.OnHttpRequest(e, world);
                    }
                    if (pathname.StartsWith("/events"))
                    {
                        Task t;
                        lock (session.new_events)
                        {
                            //Write events
                            t = QuickWriteJsonToDoc(e, session.new_events);

                            //Nuke events
                            session.new_events.Clear();
                        }

                        //Return task
                        return t;
                    }
                    if (pathname.StartsWith("/tribes/item_search/"))
                    {
                        //Search for items.
                        string query = e.Request.Query["q"].ToString().ToLower();

                        //Get page offset
                        int page_offset = int.Parse(e.Request.Query["p"]);

                        //Reverse-search this name and find classnames
                        var itemEntries = ArkImports.item_entries.Where(x => x.name.ToLower().Contains(query)).ToArray();

                        //Sort item entries by length closest to the one entered.
                        Array.Sort(itemEntries, delegate (ArkItemEntry x, ArkItemEntry y) { return x.name.Length.CompareTo(y.name.Length); });

                        //Now use the classnames from this to search for items.
                        List<ArkItemSearchResultsItem> tribeItems = new List<ArkItemSearchResultsItem>();
                        int addedIndex = 0;
                        for(int i = 0; i<itemEntries.Length; i++)
                        {
                            var item = itemEntries[i];
                            bool add = session.item_dict_cache.ContainsKey(item.classname);
                            if (addedIndex >= page_offset * TRIBE_ITEMS_MAX_PAGE_RESULTS && addedIndex < (page_offset+1) * TRIBE_ITEMS_MAX_PAGE_RESULTS && add)
                            {
                                tribeItems.Add(session.item_dict_cache[item.classname]);
                            }
                            if(add)
                            {
                                addedIndex++;
                            }
                        }

                        //Generate an output.
                        int tribeItemsCount = tribeItems.Count;
                        bool moreListItems = addedIndex > (page_offset + 1) * TRIBE_ITEMS_MAX_PAGE_RESULTS;

                        ArkItemSearchResults output = new ArkItemSearchResults
                        {
                            hardLimit = TRIBE_ITEMS_MAX_PAGE_RESULTS,
                            itemEntriesFound = itemEntries.Length,
                            moreListItems = moreListItems,
                            tribeItemsFound = tribeItemsCount,
                            results = tribeItems,
                            query = query
                        };

                        //Write
                        return QuickWriteJsonToDoc(e, output);
                    }
                    if (pathname.StartsWith("/tribes/"))
                    {
                        //Convert to a basic Ark world
                        BasicTribe bworld = new BasicTribe(world, session.tribe_id, sessionId, session.last_dino_list);

                        //Update last dino list
                        List<string> dino_ids = new List<string>();
                        foreach (var d in bworld.dinos.Values)
                            dino_ids.Add(d.id.ToString());
                        session.last_dino_list = dino_ids;

                        //Write
                        return QuickWriteJsonToDoc(e, bworld);
                    }
                    if (pathname.StartsWith("/dinos/"))
                    {
                        //Get the dino ID
                        string id = pathname.Substring("/dinos/".Length);
                        //Parse this into a Dino ID
                        if (!ulong.TryParse(id, out ulong dinoid))
                            //Failed.
                            return QuickWriteToDoc(e, "Failed to parse dinosaur ID.", "text/plain", 400);
                        //Search with this dinosaur ID
                        var dinos = world.dinos.Where(x => x.dinosaurId == dinoid).ToArray();
                        if (dinos.Length == 1)
                        {
                            //Write this dinosaur.
                            return QuickWriteJsonToDoc(e, new ArkDinoReply(dinos[0], world));
                        }
                        else
                        {
                            //Failed to find.
                            return QuickWriteToDoc(e, $"The dinosaur ID '{dinoid}' was not a valid dinosaur.", "text/plain", 404);
                        }
                    }
                }

                //No path exists here.
                return QuickWriteToDoc(e, "Not Found at " + pathname, "text/plain", 404);
            } catch (Exception ex)
            {
                return QuickWriteJsonToDoc(e, new ServerErrorReturn
                {
                    caught = false,
                    message = ex.Message,
                    stack = ex.StackTrace
                }, 500);
            }
        }

        public static Task OnCreateSessionRequest(Microsoft.AspNetCore.Http.HttpContext e, MasterServerArkUser user)
        {
            //Generate a random session ID
            string sessionId = GenerateRandomString(8).ToLower();
            while(sessions.ContainsKey(sessionId))
                sessionId = GenerateRandomString(8).ToLower();

            //Get pathname
            string file_path = config.save_location;

            //Get Ark world
            ArkWorld w = new ArkWorld(ArkSaveEditor.Deserializer.ArkSaveDeserializer.OpenDotArk(file_path));

            //Create session
            var session = new HttpSession
            {
                world = w,
                new_events = new List<HttpSessionEvent>(),
                game_file_path = file_path,
                last_heartbeat_time = DateTime.UtcNow,
                session_id = sessionId,
                worldLastSavedAt = File.GetLastWriteTimeUtc(file_path),
                latest_user = user
            };

            //Find tribe ID
            session.FindTribeId(user);

            //Recompute dino dict
            session.RecomputeItemDictCache(w.dinos.Where(x => x.isTamed == true && x.tribeId == session.tribe_id).ToList());

            //Recompute hash
            session.last_file_hash = session.GetComputedFileHash();

            //Add to sessions
            lock(sessions)
                sessions.Add(sessionId, session);

            //Return basic Ark world
            return QuickWriteJsonToDoc(e, new BasicArkWorld(w, session));
        }
    }
}
