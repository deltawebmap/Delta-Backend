var map = {};
map.map = null;
map.layerList = {};

map.populationMapFilter = "";

map.init = function() {
    //Create map
    map.map = L.map('map_part', {
        crs: L.CRS.Simple,
        minZoom: 0
    }).setView([-128, 128], 2);

    

    //Create main population
    /*L.tileLayer('https://ark.romanport.com/api/map/tiles/population/?z={z}&x={x}&y={y}&filter=', {
        attribution: 'AllGameMaps.com',
        maxZoom: 5,
        id: 'ark_map',
        opacity: 1,
        zIndex: 1
    }).addTo(map.map);*/
};

map.resetPopulationMap = function(isOn, filteredClassname) {
    //Remove layer if needed
    if(map.layerList["population_map"] != null) {
        try {
            map.layerList["population_map"].removeFrom(map.map);
        } catch {

        }
    }

    //If it is on, add it
    if(isOn) {
        map.addPopulationMap(filteredClassname);
    }
}

map.addGameMapLayer = function() {
    //Create main tile layer
    L.tileLayer(ark.session.endpoint_game_map, {
        attribution: 'AllGameMaps.com',
        maxNativeZoom: 5,
        maxZoom:12,
        id: 'ark_map',
        opacity: 1,
        zIndex: 1,
        bounds:map.getBounds()
    }).addTo(map.map);
}

map.onPopulationMapToggle = function(value) {
    if(value == false) {
        //Remove
        map.layerList["population_map"].removeFrom(map.map);
    } else {
        //Add
        map.addPopulationMap(map.populationMapFilter);
    }
}

map.getBounds = function() {
    return [
        [-256, 0],
        [0, 256]
    ];
}

map.addPopulationMap = function(filter) {
    var url = ark.session.endpoint_population_map.replace("{filter}", encodeURIComponent(filter));
    map.layerList["population_map"] = L.tileLayer(url, {
        maxZoom: 50,
        id: 'ark_map',
        opacity: 1,
        zIndex: 2,
        bounds: map.getBounds()
    });
    map.layerList["population_map"].addTo(map.map);
}

map.onChangePopulationMapFilter = function(dinoData) {
    //Remove
    if(map.layerList["population_map"] != null) {
        map.layerList["population_map"].removeFrom(map.map);
    }
    //Activate
    document.getElementById('population_map_check').checked = true;
    if(dinoData == null) {
        //No filter
        map.addPopulationMap("");
    } else {
        //Dino filter
        map.addPopulationMap(dinoData.classname);
    }
}

//Key: Dino ID, 
map.dino_marker_list = {};
map.dino_marker_list_index = 0;

/* Tribe dinos */
map.onEnableTribeDinos = function() {
    //Query tribe dinos
    ark.serverRequest(ark.session.endpoint_tribes, {"customErrorText":"Failed to refresh tribe data."}, function(d) {
        //Update or add existing dinos
        /*for(var i = 0; i<d.diff_dinos_added.length; i+=1) {
            var dino_id = d.diff_dinos_added[i];
            var dino = d.dinos[dino_id];
            if(dino == null) {
                console.warn("Warning: Dino ID "+dino_id+" was not found, but was referenced.");
            }
            //Create marker
            map.addDinoMarker(dino);
        }

        //Remove old, dead dinos D:
        if(d.diff_dinos_missing != null) {
            for(var i = 0; i<d.diff_dinos_missing.length; i+=1) {
                var dino_id = d.diff_dinos_missing[i];
                map.removeDinoMarkerById(dino_id);
                console.log("So long, "+dino_id);
            }
        }*/

        //Add dinos
        for(var i = 0; i<d.current_dino_ids.length; i+=1) {
            var dino_id = d.current_dino_ids[i];
            var dino = d.dinos[dino_id];
            if(dino == null) {
                console.warn("Warning: Dino ID "+dino_id+" was not found, but was referenced.");
            }
            //Create marker
            map.addDinoMarker(dino);
        }

        //Add babies
        for(var i = 0; i<d.baby_dino_urls.length; i+=1) {
            bman.addDinoTimer(d.baby_dino_urls[i]);
        }

        //Start rendering map layer. We waited to save bandwidth.
        map.addGameMapLayer();
    });
}

map.addDinoMarker = function(dino) {
    var icon = L.icon({
        iconUrl: dino.imgUrl,
        shadowUrl: null,
    
        iconSize:     [30, 30], // size of the icon
        shadowSize:   [0, 0], // size of the shadow
        iconAnchor:   [15, 15+43], // point of the icon which will correspond to marker's location
        shadowAnchor: [15, 15],  // the same for the shadow
        popupAnchor:  [15, 15] // point from which the popup should open relative to the iconAnchor
    });
    var background_icon = L.icon({
        iconUrl: "https://ark.romanport.com/resources/dino_cursor.png",
        shadowUrl: null,
        
    
        iconSize:     [50, 70], // size of the icon
        shadowSize:   [0, 0], // size of the shadow
        iconAnchor:   [25, 67], // point of the icon which will correspond to marker's location
        shadowAnchor: [15, 15],  // the same for the shadow
        popupAnchor:  [15, 15] // point from which the popup should open relative to the iconAnchor
    });
    //Add to map
    ///This map is weird. 0,0 is the top right, while -256, 256 is the bottom right corner. Invert x
    var pos = [
        (-dino.adjusted_map_pos.y * 256),
        (dino.adjusted_map_pos.x * 256)
    ];

    var index = map.dino_marker_list_index * 2;

    //Check if this dino is already in the list. If it is, update the zindex to match
    if(map.dino_marker_list[dino.id] != null) {
        //index = map.dino_marker_list[dino.id] * 2;
        map.dino_marker_list_index+=1;
    } else {
        map.dino_marker_list_index+=1;
    }
    

    var dino_icon = L.marker(pos, {
        icon: icon,
        zIndexOffset:index+1
    }).addTo(map.map);
    var dino_icon_bg = L.marker(pos, {
        icon: background_icon,
        zIndexOffset:index
    }).addTo(map.map);

    //Add items
    dino_icon.x_dino_url = dino.apiUrl;
    dino_icon_bg.x_dino_url = dino.apiUrl;

    //Add events
    dino_icon.on('click', map.onDinoClicked);
    dino_icon_bg.on('click', map.onDinoClicked);

    //Remove existing markers if they exist
    map.removeDinoMarkerById(dino.id);
    
    //Add to list
    map.dino_marker_list[dino.id] = {
        "map_icons":[
            dino_icon,
            dino_icon_bg
        ],
        "index":index/2
    }
}

map.removeDinoMarkerById = function(id) {
    if(map.dino_marker_list[id] != null) {
        var d = map.dino_marker_list[id];
        for(var i = 0; i<d.map_icons.length; i+=1) {
            d.map_icons[i].removeFrom(map.map);
        }
        d.map_icons = [];
    }
}


map.onDinoClicked = function(e) {
    var url = this.x_dino_url;

    //Close any existing
    map_menu.hide();

    //Open
    map_menu.show(url);
}