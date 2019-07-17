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
using ArkWebMapLightspeedClient;
using ArkWebMapLightspeedClient.Entities;

namespace ArkHttpServer
{
    partial class ArkWebServer
    {
        public const int TRIBE_ITEMS_MAX_PAGE_RESULTS = 10;

        public const int CURRENT_CLIENT_VERSION = 1;

        public static async Task OnHttpRequest(LightspeedRequest e, MasterServerArkUser user)
        {
            try
            {
                //Get world
                ArkWorld world = WorldLoader.GetWorld();

                //Look up user based on Steam ID.
                bool isInTribe = false;
                int tribeId = -1;
                var foundPlayers = world.players.Where(x => x.steamPlayerId == user.steam_id).ToArray();
                if (foundPlayers.Length >= 1)
                {
                    tribeId = foundPlayers[0].tribeId;
                    isInTribe = true;
                }

                //Since we don't have to worry about permissions or anything, we'll just have a list of services created at compile time.
                string pathname = e.endpoint.ToLower();

                //Handle things that are not map specific
                if (pathname.StartsWith("/create_session"))
                {
                    //Create a new session. For now, just use the same map.
                    await OnCreateSessionRequest(e, user);
                    return;
                }
                if (pathname.StartsWith("/dino_search/"))
                {
                    //Search for dinos in the dino entry table.
                    string query = e.query["query"].ToString().ToLower();
                    List<ArkDinoEntry> dinos = new List<ArkDinoEntry>();
                    foreach (var d in ArkImports.dino_entries)
                    {
                        if (d.screen_name.ToLower().Contains(query))
                            dinos.Add(d);
                        if (dinos.Count >= 10)
                            break;
                    }
                    await e.DoRespondJson(new DinoSearchReply
                    {
                        query = query,
                        results = dinos
                    });
                    return;
                }

                //Continue to map specific things
                if (pathname.StartsWith("/world/"))
                {
                    //Fix pathname
                    pathname = pathname.Substring("/world".Length);
                    
                    //If this is a demo server, set the tribe ID to the demo tribe.
                    if(ArkWebServer.config.is_demo_server)
                    {
                        isInTribe = true;
                        tribeId = ArkWebServer.config.demo_tribe_id;
                    }

                    if (pathname.StartsWith("/map/tiles/population/") && ArkWebServer.CheckPermission("allowHeatmap"))
                    {
                        await PopulationService.OnHttpRequest(e, world);
                        return;
                    }
                    if (pathname.StartsWith("/tribes/item_search/") && ArkWebServer.CheckPermission("allowSearchTamedTribeDinoInventories"))
                    {
                        await TribeInventorySearchService.OnHttpRequest(e, world, tribeId);
                        return;
                    }
                    if(pathname == "/tribes/overview")
                    {
                        await TribeOverviewService.OnHttpRequest(e, world, tribeId);
                        return;
                    }
                    if(pathname == "/tribes/hub")
                    {
                        //Hub for the frontpage.
                        await TribeHubService.OnHttpRequest(e, world, tribeId);
                        return;
                    }
                    if (pathname.StartsWith("/tribes/"))
                    {
                        //Convert to a basic Ark world
                        BasicTribe bworld = new BasicTribe(world, tribeId);

                        //Write
                        await e.DoRespondJson(bworld);
                        return;
                    }
                    if (pathname.StartsWith("/dinos/") && ArkWebServer.CheckPermission("allowViewTamedTribeDinoStats"))
                    {
                        //Get the dino ID
                        string id = pathname.Substring("/dinos/".Length);
                        //Parse this into a Dino ID
                        if (!ulong.TryParse(id, out ulong dinoid))
                        {
                            //Failed.
                            await e.DoRespondString("Failed to parse dinosaur ID.", "text/plain", 400);
                            return;
                        }
                        //Search with this dinosaur ID
                        var dinos = world.dinos.Where(x => x.dinosaurId == dinoid && x.tribeId == tribeId).ToArray();
                        if (dinos.Length > 0)
                        {
                            //Get the method
                            var method = ArkWebServer.FindRequestMethod(e);
                            if(method == RequestHttpMethod.post)
                            {
                                //Edit dino
                                DinoServerData newSettings = ArkWebServer.DecodePostBody<DinoServerData>(e);
                                newSettings.dinoId = dinos[0].dinosaurId.ToString();
                                newSettings.tribeId = dinos[0].tribeId;
                            }
                            //Write this dinosaur.
                            await e.DoRespondJson(new ArkDinoReply(dinos[0], world));
                            return;
                        }
                        else
                        {
                            //Failed to find.
                            await e.DoRespondString($"The dinosaur ID '{dinoid}' was not a valid dinosaur.", "text/plain", 404);
                            return;
                        }
                    }
                    
                }

                //No path exists here.
                await e.DoRespondString("Not Found at " + pathname, "text/plain", 404);
                return;
            } catch (Exception ex)
            {
                await e.DoRespondJson(new ServerErrorReturn
                {
                    caught = false,
                    message = ex.Message,
                    stack = ex.StackTrace
                }, 500);
                return;
            }
        }

        public static async Task OnCreateSessionRequest(LightspeedRequest e, MasterServerArkUser user)
        {
            //Return basic Ark world
            ArkWorld world = WorldLoader.GetWorld(out DateTime lastSavedTime);
            await e.DoRespondJson(new BasicArkWorld(world, lastSavedTime));
        }
    }
}
