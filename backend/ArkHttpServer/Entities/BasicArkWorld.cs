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
        public double mapTimeOffset; //Offset from the map time sent. Usually obtained by getting the amount of time since now and the last time the file was saved.
        public bool isDemoServer;
        public string mapBackgroundColor;

        public string href; //URL of this file. Depending on how this was loaded, this might be different from what was actually requested.
        public string endpoint_population_map; //Endpoint for viewing populations
        public string endpoint_game_map; //Endpoint for viewing the actual game map
        public string endpoint_tribes; //Endpoint for viewing tribes
        public string endpoint_dino_class_search;
        public int heartrate; //Time, in ms, that the client must use to poll for events.
        public string endpoint_events; //Events endpoint
        public string endpoint_tribes_itemsearch; //Item search endpoint
        public string endpoint_tribes_dino; //Dino endpoint
        public string endpoint_tribes_overview; //Tribe properties list

        public ServerPermissionsRole permissions;

        public BasicArkWorld(ArkWorld w, DateTime lastSavedAt)
        {
            //Set world data
            day = w.day;
            dayTime = w.gameTime;
            mapName = w.map;
            mapData = w.mapinfo;
            isDemoServer = ArkWebServer.config.is_demo_server;
            mapBackgroundColor = w.mapinfo.backgroundColor;

            //Calculate map time offset
            mapTimeOffset = (DateTime.UtcNow - lastSavedAt).TotalSeconds;

            //Set endpoints
            string baseUrl = $"{ArkWebServer.api_prefix}/world/";
            href = baseUrl;
            endpoint_population_map = baseUrl + "map/tiles/population/?zoom={z}&x={x}&y={y}&filter={filter}&v="+ArkWebServer.CURRENT_CLIENT_VERSION;
            endpoint_game_map = $"https://ark.romanport.com/resources/maps/"+mapName+"/tiles/{z}_{x}_{y}.png";
            //endpoint_game_map = "http://localhost:84/{z}_{x}_{y}.png";
            endpoint_tribes = baseUrl + "tribes/";
            heartrate = ArkWebServer.EVENTS_HEARTRATE;
            endpoint_events = baseUrl + "events?t="+DateTime.UtcNow.Ticks;
            endpoint_tribes_itemsearch = baseUrl + "tribes/item_search/?q={query}";
            endpoint_tribes_dino = baseUrl + "dinos/{dino}";
            endpoint_dino_class_search = $"{ArkWebServer.api_prefix}/dino_search/?query={{query}}";
            endpoint_tribes_overview = baseUrl + "tribes/overview";

            permissions = ArkWebServer.config.base_permissions;

            //If this is a demo server, reset the time offsets
            if(ArkWebServer.config.is_demo_server)
            {
                mapTimeOffset = 0;
            }
        }
    }
}
