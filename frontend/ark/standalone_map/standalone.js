var st = {};
st.settings = {};

st.init = function() {
    //Get settings from hash
    var settingsString = window.location.hash.replace('#', '');
    st.settings = JSON.parse(decodeURIComponent(settingsString));

    //Create map
    st.map = L.map('map_part', {
        crs: L.CRS.Simple,
        minZoom: 0
    }).setView([-128, 128], 2);

    //Add map layer
    L.tileLayer(st.settings.mapUrl, {
        attribution: 'AllGameMaps.com',
        maxNativeZoom: 5,
        maxZoom:12,
        id: 'ark_map',
        opacity: 1,
        zIndex: 1,
        bounds:st.getBounds()
    }).addTo(st.map);

    //Add dinos
    for(var i = 0; i<st.settings.dinos.length; i+=1) {
        st.addDinoMarker(st.settings.dinos[i]);
    }

    //Tell client we are ready
    app.onMapReady();   
}

window.onhashchange = function() {
	//Updated. Close map and reinit
    st.map.remove();
    st.init();
}

st.getBounds = function() {
    return [
        [-256, 0],
        [0, 256]
    ];
}

st.dino_marker_list_index = 0;
st.addDinoMarker = function(dino) {
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

    var index = st.dino_marker_list_index * 2;

    var dino_icon = L.marker(pos, {
        icon: icon,
        zIndexOffset:index+1
    }).addTo(st.map);
    var dino_icon_bg = L.marker(pos, {
        icon: background_icon,
        zIndexOffset:index
    }).addTo(st.map);

    //Add items
    dino_icon.x_dino_url = dino.apiUrl;
    dino_icon_bg.x_dino_url = dino.apiUrl;

    //Add events
    dino_icon.on('click', st.onDinoClicked);
    dino_icon_bg.on('click', st.onDinoClicked);
}

st.onDinoClicked = function() {
    var url = this.x_dino_url;
    app.onDinoClicked(url);
}