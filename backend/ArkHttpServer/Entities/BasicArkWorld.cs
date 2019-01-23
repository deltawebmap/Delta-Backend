using ArkSaveEditor.Entities;
using ArkSaveEditor.World;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkHttpServer.Entities
{
    public class BasicArkWorld
    {
        public int day;
        public float dayTime;
        public string mapName;
        public ArkMapData mapData;

        public string href; //URL of this file. Depending on how this was loaded, this might be different from what was actually requested.
        public string endpoint_population_map; //Endpoint for viewing populations
        public string endpoint_game_map; //Endpoint for viewing the actual game map
        public string endpoint_tribes; //Endpoint for viewing tribes
        public int heartrate; //Time, in ms, that the client must use to poll for events.
        public string endpoint_events; //Events endpoint
        public string endpoint_tribes_itemsearch; //Item search endpoint

        public BasicArkWorld(ArkWorld w, string sessionId)
        {
            //Set world data
            day = w.day;
            dayTime = w.gameTime;
            mapName = w.map;
            mapData = w.mapinfo;

            //Set endpoints
            string baseUrl = $"https://ark.romanport.com/api/world/{sessionId}/";
            href = baseUrl;
            endpoint_population_map = baseUrl + "map/tiles/population/?zoom={z}&x={x}&y={y}&filter={filter}&v="+Program.CURRENT_CLIENT_VERSION;
            endpoint_game_map = "https://ark.romanport.com/resources/maps/"+mapName+"/tiles/{z}_{x}_{y}.png";
            endpoint_tribes = baseUrl + "tribes/";
            heartrate = Program.SESSION_TIMEOUT_MS;
            endpoint_events = baseUrl + "events";
            endpoint_tribes_itemsearch = baseUrl + "tribes/item_search/?q={query}";
        }
    }
}
