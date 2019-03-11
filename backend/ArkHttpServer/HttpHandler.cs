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
        public const int TRIBE_ITEMS_MAX_PAGE_RESULTS = 10;

        public const int CURRENT_CLIENT_VERSION = 1;

        public static Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, MasterServerArkUser user)
        {
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
                        if (dinos.Count >= 10)
                            break;
                    }
                    return ArkWebServer.QuickWriteJsonToDoc(e, new DinoSearchReply
                    {
                        query = query,
                        results = dinos
                    });
                }

                //Continue to map specific things
                if (pathname.StartsWith("/world/"))
                {
                    //Get world
                    ArkWorld world = WorldLoader.GetWorld();

                    //Fix pathname
                    pathname = pathname.Substring("/world".Length);

                    //Look up user based on Steam ID.
                    bool isInTribe = false;
                    int tribeId = -1;
                    var foundPlayers = world.players.Where(x => x.steamPlayerId == user.steam_id).ToArray();
                    if(foundPlayers.Length >= 1)
                    {
                        tribeId = foundPlayers[0].tribeId;
                        isInTribe = true;
                    }
                    
                    //If this is a demo server, set the tribe ID to the demo tribe.
                    if(ArkWebServer.config.is_demo_server)
                    {
                        isInTribe = true;
                        tribeId = ArkWebServer.config.demo_tribe_id;
                    }

                    if (pathname.StartsWith("/map/tiles/population/") && ArkWebServer.CheckPermission("allowHeatmap"))
                    {
                        return PopulationService.OnHttpRequest(e, world);
                    }
                    if (pathname.StartsWith("/events"))
                    {
                        return EventService.OnEventRequest(e, user);
                    }
                    if (pathname.StartsWith("/tribes/item_search/") && ArkWebServer.CheckPermission("allowSearchTamedTribeDinoInventories"))
                    {
                        return TribeInventorySearchService.OnHttpRequest(e, world, tribeId);
                    }
                    if(pathname == "/tribes/overview")
                    {
                        return TribeOverviewService.OnHttpRequest(e, world, tribeId);
                    }
                    if (pathname.StartsWith("/tribes/"))
                    {
                        //Convert to a basic Ark world
                        BasicTribe bworld = new BasicTribe(world, tribeId);

                        //Write
                        return QuickWriteJsonToDoc(e, bworld);
                    }
                    if (pathname.StartsWith("/dinos/") && ArkWebServer.CheckPermission("allowViewTamedTribeDinoStats"))
                    {
                        //Get the dino ID
                        string id = pathname.Substring("/dinos/".Length);
                        //Parse this into a Dino ID
                        if (!ulong.TryParse(id, out ulong dinoid))
                            //Failed.
                            return QuickWriteToDoc(e, "Failed to parse dinosaur ID.", "text/plain", 400);
                        //Search with this dinosaur ID
                        var dinos = world.dinos.Where(x => x.dinosaurId == dinoid && x.tribeId == tribeId).ToArray();
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
            //Return basic Ark world
            ArkWorld world = WorldLoader.GetWorld(out DateTime lastSavedTime);
            return QuickWriteJsonToDoc(e, new BasicArkWorld(world, lastSavedTime));
        }
    }
}
