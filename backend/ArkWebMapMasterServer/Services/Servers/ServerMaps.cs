using ArkBridgeSharedEntities.Entities;
using ArkWebMapGatewayClient.Messages;
using ArkWebMapGatewayClient.Messages.Entities;
using ArkWebMapMasterServer.NetEntities;
using ArkWebMapMasterServer.PresistEntities;
using ArkWebMapMasterServer.Tools;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Servers
{
    public static class ServerMaps
    {
        public static Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, ArkServer s, int tribeId, string next)
        {
            //Grab the maps for this server
            SavedMapEntry[] maps = DrawableMapTool.GetServerMaps(s._id, tribeId);

            //Find what to do
            RequestHttpMethod method = Program.FindRequestMethod(e);
            int mapId = -1;
            if(next.StartsWith('/'))
            {
                if (!int.TryParse(next.Substring(1), out mapId))
                    mapId = -1;
            }

            //Do action
            if (method == RequestHttpMethod.get)
            {
                //Get either the map list, or a map
                return OnGetRequest(e, s, tribeId, maps, mapId);
            } else if (method == RequestHttpMethod.post)
            {
                //Create a new map, or edit an existing map
                return OnPostRequest(e, s, tribeId, maps, mapId);
            } else if (method == RequestHttpMethod.delete)
            {
                //Destroy an existing map, clearing the points
                return OnDeleteRequest(e, s, tribeId, maps, mapId);
            }
            throw new StandardError("This method is not supported by this endpoint.", StandardErrorCode.BadMethod);
        }

        private static SavedMapEntry GetMapById(SavedMapEntry[] maps, int id)
        {
            foreach(var m in maps)
            {
                if (m.map_id == id)
                    return m;
            }
            return null;
        }

        private static Task OnGetRequest(Microsoft.AspNetCore.Http.HttpContext e, ArkServer s, int tribeId, SavedMapEntry[] maps, int mapId)
        {
            //If we're requesting a map ID, return it. Else, return the list of maps.
            if(mapId == -1)
            {
                //Return list
                MapListRequestResponse response = new MapListRequestResponse
                {
                    maps = new MapListRequestResponseEntry[maps.Length]
                };
                for(int i = 0; i<maps.Length; i+=1)
                {
                    SavedMapEntry map = maps[i];
                    response.maps[i] = new MapListRequestResponseEntry
                    {
                        id = map.map_id,
                        name = map.map_name,
                        url = $"{Program.PREFIX_URL}/servers/{s._id}/maps/{map.map_id}"
                    };
                }

                //Write response
                return Program.QuickWriteJsonToDoc(e, response);
            } else
            {
                //Fetch map data
                SavedMapEntry map = GetMapById(maps, mapId);
                if (map == null)
                    throw new StandardError("This map ID was not found.", StandardErrorCode.NotFound);

                //Fetch the map points
                List<ArkTribeMapDrawingPoint> points = DrawableMapTool.GetMapPoints(map.server_id, map.tribe_id, map.map_id);
                MapRequestResponse response = new MapRequestResponse
                {
                    id = map.map_id,
                    name = map.map_name,
                    points = points
                };

                //Write response
                return Program.QuickWriteJsonToDoc(e, response);
            }
        }

        private static Task OnPostRequest(Microsoft.AspNetCore.Http.HttpContext e, ArkServer s, int tribeId, SavedMapEntry[] maps, int mapId)
        {
            //Decode body
            DrawableMapEditRequest body = Program.DecodePostBody<DrawableMapEditRequest>(e);

            //If we're requesting a map ID, edit it. If not, create a new map.
            if (mapId == -1)
            {
                //Create a new map. Find the first ID we can use
                int id = 0;
                while (GetMapById(maps, id) != null)
                    id++;

                //Create a map entry
                SavedMapEntry map = new SavedMapEntry
                {
                    map_id = id,
                    map_name = body.name,
                    server_id = s._id,
                    tribe_id = tribeId
                };

                //Add
                DrawableMapTool.GetMapsCollection().Insert(map);

                //Clear in case there is any remaining data for some reason
                DrawableMapTool.ClearMapPoints(map.server_id, map.tribe_id, map.map_id);

                //Send action
                GatewayActionTool.SendActionToTribe(new MessageOnDrawableMapChange
                {
                    opcode = ArkWebMapGatewayClient.GatewayMessageOpcode.OnDrawableMapChange,
                    id = map.map_id,
                    name = map.map_name,
                    mapOpcode = MessageOnDrawableMapChange.MessageOnDrawableMapChangeOpcode.Create
                }, map.tribe_id, map.server_id);

                //Return the new ID
                DrawableMapEditResponse response = new DrawableMapEditResponse
                {
                    id = id
                };
                return Program.QuickWriteJsonToDoc(e, response);
            } else
            {
                //Fetch map data
                SavedMapEntry map = GetMapById(maps, mapId);
                if (map == null)
                    throw new StandardError("This map ID was not found.", StandardErrorCode.NotFound);

                //Update
                map.map_name = body.name;
                if (body.doClear)
                    DrawableMapTool.ClearMapPoints(map.server_id, map.tribe_id, map.map_id);
                DrawableMapTool.GetMapsCollection().Update(map);

                //Send rename action
                GatewayActionTool.SendActionToTribe(new MessageOnDrawableMapChange
                {
                    opcode = ArkWebMapGatewayClient.GatewayMessageOpcode.OnDrawableMapChange,
                    id = map.map_id,
                    name = map.map_name,
                    mapOpcode = MessageOnDrawableMapChange.MessageOnDrawableMapChangeOpcode.Rename
                }, map.tribe_id, map.server_id);

                //Send clear message
                if(body.doClear)
                {
                    //Send action
                    GatewayActionTool.SendActionToTribe(new MessageOnDrawableMapChange
                    {
                        opcode = ArkWebMapGatewayClient.GatewayMessageOpcode.OnDrawableMapChange,
                        id = map.map_id,
                        name = map.map_name,
                        mapOpcode = MessageOnDrawableMapChange.MessageOnDrawableMapChangeOpcode.Clear
                    }, map.tribe_id, map.server_id);
                }

                //Return the ID and update
                DrawableMapEditResponse response = new DrawableMapEditResponse
                {
                    id = map.map_id
                };
                return Program.QuickWriteJsonToDoc(e, response);
            }
        }

        private static Task OnDeleteRequest(Microsoft.AspNetCore.Http.HttpContext e, ArkServer s, int tribeId, SavedMapEntry[] maps, int mapId)
        {
            //Fetch map data
            SavedMapEntry map = GetMapById(maps, mapId);
            if (map == null)
                throw new StandardError("This map ID was not found.", StandardErrorCode.NotFound);

            //Clear
            DrawableMapTool.ClearMapPoints(map.server_id, map.tribe_id, map.map_id);

            //Delete entry
            DrawableMapTool.GetMapsCollection().Delete(map._id);

            //Send action
            GatewayActionTool.SendActionToTribe(new MessageOnDrawableMapChange
            {
                opcode = ArkWebMapGatewayClient.GatewayMessageOpcode.OnDrawableMapChange,
                id = map.map_id,
                name = map.map_name,
                mapOpcode = MessageOnDrawableMapChange.MessageOnDrawableMapChangeOpcode.Delete
            }, map.tribe_id, map.server_id);

            //Return OK
            return Program.QuickWriteStatusToDoc(e, true);
        }
    }
}
