var CLIENT_VERSION = 1;

var ark = {};
ark.session = null;
ark.session_events_url = null;
ark.session_start_time = null;

ark.statusEntries = {
    "water": {
        "icon":"/resources/ui/status/water.png",
        "name":"Water"
    },
    "unknown3": {
        "icon":"/resources/ui/status/unknown3.png",
        "name":"UNKNOWN 3"
    },
    "unknown2": {
        "icon":"/resources/ui/status/unknown2.png",
        "name":"UNKNOWN 2"
    },
    "unknown1": {
        "icon":"/resources/ui/status/unknown1.png",
        "name":"UNKNOWN 1"
    },
    "stamina": {
        "icon":"/resources/ui/status/stamina.png",
        "name":"Stamina"
    },
    "oxygen": {
        "icon":"/resources/ui/status/oxygen.png",
        "name":"Oxygen"
    },
    "movementSpeedMult": {
        "icon":"/resources/ui/status/movementSpeedMult.png",
        "name":"Movement Speed"
    },
    "meleeDamageMult": {
        "icon":"/resources/ui/status/meleeDamageMult.png",
        "name":"Melee Damage"
    },
    "inventoryWeight": {
        "icon":"/resources/ui/status/inventoryWeight.png",
        "name":"Weight"
    },
    "health": {
        "icon":"/resources/ui/status/health.png",
        "name":"Health"
    },
    "food": {
        "icon":"/resources/ui/status/food.png",
        "name":"Food"
    },
};

ark.serverRequest = function(url, args, callback) {
    var xmlhttp = new XMLHttpRequest();
    xmlhttp.onreadystatechange = function () {
        if (this.readyState == 4 && this.status == 200) {
            callback(JSON.parse(this.responseText));
        } else if (this.readyState == 4 && this.status == 502) {
            //Host offline
            bottom_modal.forceHideBottomModalNoArgs();
            bottom_modal.reportError("This server is offline. Please try again later.");
        } else if (this.readyState == 4 && this.status == 500) {
            //Known server error.
            var err = JSON.parse(this.responseText);
            bottom_modal.forceHideBottomModalNoArgs();
            bottom_modal.reportError("Couldn't fetch data.\n\nError: "+err.message);
        } else if (this.readyState == 4 && this.status == 521) {
            //This is the error code returned by the Ark master server proxy. Abort.
            ark.onConnectedServerStop();
        } else if (this.readyState == 4) {
            //Parse the response and display the error
            ark.onNetError(this.status + " ("+this.statusText+")", args);
        }
    }
    xmlhttp.ontimeout = function () {
        ark.onNetError("Timed out", args);
    }
    xmlhttp.onerror = function () {
        ark.onNetError("Generic error", args);
    }
    xmlhttp.onabort = function () {
        ark.onNetError("Request aborted", args);
    }
    if(args.type == null) {
        args.type = "GET";
    }
    if(url.includes("?")) {
        url += "&v="+CLIENT_VERSION;
    } else {
        url += "?v="+CLIENT_VERSION;
    }
    xmlhttp.open(args.type, url, true);
    xmlhttp.withCredentials = true;
    if(args.nocreds != null) {
        xmlhttp.withCredentials = !args.nocreds;
    }
    xmlhttp.send(args.body);
}

ark.onNetError = function(errorCode, args) {
    bottom_modal.forceHideBottomModalNoArgs();
    if(args.customErrorText != null) {
        bottom_modal.reportError(args.customErrorText+" Error code: "+errorCode);
    } else {
        bottom_modal.reportError("Couldn't fetch data. Error code: "+errorCode);
    }
    
}

ark.openSession = function(callback, url) {
    ark.serverRequest(url, {"customErrorText":"Failed to create session."}, function(d) {
        ark.session = d;
        ark.session_events_url = d.endpoint_events;

        //Reset start time
        ark.session_start_time = new Date();

        //Set last updated time
        ark.updateLastEditedUi();

        //Start running events
        ark.session.eventLoop = window.setInterval(ark.onHeartbeat, d.heartrate);
        callback(d);
    });
}

ark.getCurrentGameTime = function() {
    //Calculate the current game time
    return ark.session.dayTime + ark.session.mapTimeOffset + ((new Date() - ark.session_start_time) / 1000);
}

ark.getGameTimeOffset = function() {
    //Calculate the offset from the file time to the current time. This is to calculate the in-game values, even without a world save.
    return ark.getCurrentGameTime() - ark.session.dayTime;
}

ark.onHeartbeat = function() {
    //Send a request to the server
    ark.serverRequest(ark.session_events_url, {"customErrorText":"Failed to refresh heartbeat and events."}, function(d) {
        //Set next url
        ark.session_events_url = d.next_url;
        //Process each event
        for(var i = 0; i<d.events.length; i+=1) {
            var e = d.events[i];
            var code = ark.eventDict[e.type];
            code(e.data, e.time);
        }
    });

    //Update last updated time
    ark.updateLastEditedUi();
}

ark.onEvent_MapUpdate = function(d, time) {
    console.log("GOT MAP UPDATE EVENT");

    //Reload dino locations
    map.onEnableTribeDinos();
}

ark.eventDict = {
    0: ark.onEvent_MapUpdate
};

ark.currentServer = null;

ark.switchServer = function(serverData) {
    //Ignore if we're still loading a server
    if(ark.loadingStatus != 0) {
        console.log("Will not switch server; A server is already loading!");
        return;
    }

    //Check if the server is online
    if(!serverData.has_pinged) {
        //Not pinged. Do so
        ark.serverRequest(serverData.endpoint_ping, {}, function(ping) {
            ark.onFinishPing(ping);
            ark.switchServer(document.getElementById("server_badge_"+ping.id).x_server_data);
        });
        return;
    } else {
        //If the ping failed, show message.
        if(!serverData.ping_ok) {
            ark.onClickOfflineServer(serverData.display_name, serverData.owner_uid == ark_users.me.id, serverData.id);
            return;            
        }
    }

    //If a session is already open, kill
    if(ark.session != null) {
        ark.deinitCurrentServer();
    }
    ark.currentServerId = serverData.id;

    //Remove the active markers, if any
    var activeBadges = document.getElementsByClassName('sidebar_server_badge_active');
    for(var i = 0; i<activeBadges.length; i+=1) {
        activeBadges[i].remove();
    }

    //First, open a session.
    ark.currentServer = serverData;
    ark.forceJoinServer(serverData.endpoint_createsession, serverData.id, serverData.display_name);
    
}

ark.loadingStatus = 0; //Counts down as items are loaded
//0: Done loading
//1: Awaiting the 0.5 second cooldown between switching servers
//2: Tribes endpoint loading
//3: Overview endpoint loading
//4: Main data loading

ark.forceJoinServer = function(url, id, name) {
    if(ark.loadingStatus != 0) {
        console.log("Already loading a map. Ignoring!");
        return;
    }
    //Show
    ark.setMainContentVis(true);
    ark.loadingStatus = 4;
    ark.openSession(function(session) {
        //Kill window
        ark.hideCustomArea();
        
        //Init the map
        map.init(session.endpoint_game_map);

        //Set pemrissions
        perms.p = session.permissions;
        perms.refreshPerms();

        //Clear out the tribe item search
        ark.refreshTribeItemSearch("");

        //Add active badge
        var badge = document.getElementById('server_badge_'+id);
        if(badge != null) {
            ark.createDom("div", "sidebar_server_badge_active", badge);
        }        
        
        //Fill UI
        document.getElementById('map_title').value = name;

        //Bring map title back
        document.getElementById('map_title_main').style.display = "block";
        document.getElementById('map_title_template').style.display = "none";

        //Show sidebar buttons
        ark.createAndInflateMainMenu(session);

        //Fetch tribes
        map.onEnableTribeDinos(function() { 
            document.getElementById('no_session_mask').classList.add("no_session_mask_disable");
            ark.loadingStatus--;
        });

        //Fetch overview
        dinosidebar.fetchAndGenerate(function() {
            ark.loadingStatus--;
        });

        //Loaded
        ark.loadingStatus--;

        //Add a cooldown to avoid subtle bugs
        /*window.setTimeout(function() {
            ark.loadingStatus--;
        }, 500);*/
        ark.loadingStatus--;
    }, url);
}

ark.refreshServers = function(callback) {
    ark_users.refreshUserData(function(user) { 
        //Add the users' servers
        var serverList = document.getElementById('server_list');
        serverList.innerHTML = "";
        for(var i = 0; i<user.servers.length; i+=1) {
            var s = user.servers[i];

            //Don't display if this server has never gone online
            if(!s.has_ever_gone_online || s.is_hidden) {
                continue;
            }

            var e = ark.createDom("div","sidebar_server_badge", serverList);
            var e_modal = ark.createDom("div", "");
            var ee = ark.createDom("div", "sidebar_server_badge_text", e_modal);
            var et = ark.createDom("div", "sidebar_server_badge_text_triangle", e_modal);
            e.e_modal = e_modal;

            ee.innerText = s.display_name;
            e.id = "server_badge_"+s.id;
            e.style.backgroundImage = "url("+s.image_url+")";
            e.x_server_data = s;
            e.x_server_data.has_pinged = false;
            e.addEventListener('click', function() {
                ark.switchServer(this.x_server_data);
            });
            e.addEventListener('mouseover', ark.onMouseOverSidebarServer);
            e.addEventListener('mouseout', ark.onEndMouseOverSidebarServer);

            //Start pinging
            ark.serverRequest(s.endpoint_ping, {}, ark.onFinishPing);
        }

        //Call callback
        callback(user);
    });
}

ark.onFinishPing = function(ping){
    var pele = document.getElementById("server_badge_"+ping.id);
    var ele = pele.e_modal;
    pele.x_server_data.ping_ok = ping.online;
    pele.x_server_data.has_pinged = true;
    if(ping.online) {
        //Show ping
        ele.firstChild.innerText = ping.display_name+" ("+Math.round(ping.ping).toString()+" ms)";
    } else {
        //Set to offline icon
        ele.firstChild.innerText = ping.display_name + " (Server Offline)";
        pele.style.backgroundImage = "url(/assets/server_offline.png), url("+pele.x_server_data.image_url+")";
        ele.firstChild.classList.add("badge_errbg");
        ele.lastChild.classList.add("badge_errbg_triangle");
    }
}

ark.init = function() {
    //Update user content
    ark.refreshServers(function(user) {
        console.log(user);

        //Set the icon image
        document.getElementById('my_badge').style.backgroundImage = "url("+user.profile_image_url+")";
    });

    //Create template view
    ark.createTemplateView();
}

ark.hiddenServersToUnhide = "";
ark.showHiddenServers = function() {
    //Add server entries
    ark.hiddenServersToUnhide = "";
    var p = ark.createDom("div", "");
    ark.createDom("div", "nb_title nb_big_padding_bottom", p).innerText = "Hidden Servers";
    ark.createDom("div", "np_sub_title nb_big_padding_bottom", p).innerText = "This list shows servers you've hidden from the main list. Any server running ArkWebMap that you have joined will appear here.";
    ark.createDom("div", "window_close_btn", p).addEventListener('click', function() {
        ark.serverRequest("https://ark.romanport.com/api/users/@me/servers/remove_ignore_mass/?ids="+ark.hiddenServersToUnhide, {}, function() {
            //Refresh
            ark.refreshServers(function() {
                //Hide menu
                ark.hideCustomMenu();
            })
        });
    });
    var p_list = ark.createDom("div", "scrollable archived_servers_entry_scrollable", p);
    for(var i = 0; i<ark_users.me.servers.length; i+=1) {
        var s = ark_users.me.servers[i];
        //Create entry
        var e = ark.createDom("div", "archived_servers_entry", p_list);
        ark.createDom("div", "archived_servers_entry_title", e).innerText = s.display_name;
        ark.createDom("div", "archived_servers_entry_image", e).style.backgroundImage = "url("+s.image_url+")";
        var btn = ark.createDom("div", "nb_button_blue archived_servers_entry_button nb_button_blue_inverted", e);
        btn.innerText = "Unhide Server";
        btn.x_id = s.id;
        btn.addEventListener('click', function() {
            ark.hiddenServersToUnhide += this.x_id+",";
            this.parentNode.remove();
        });
    }

    //Show
    ark.showNewCustomMenu(p, "");
}

ark.dinoPickerCallback = null;

ark.showDinoPicker = function(callback) {
    //Blur background.
    document.getElementById('main_view').className = "main_view_blurred";
    document.getElementById('html').className = "html_blurred";

    //Clear
    ark.searchDinoPicker("");

    //Set callbacks
    ark.dinoPickerCallback = callback;

    //Show dino picker
    document.getElementById('dino_selector').className = "dino_selector dino_selector_active";
}

ark.createDinoClassEntry = function(dinoData) {
    //Create html node
    var e = document.createElement('div');
    e.className = "dino_entry";

    var img = document.createElement('img');
    img.src = dinoData.icon_url;
    e.appendChild(img);

    var title = document.createElement('div');
    title.className = "dino_entry_title";
    title.innerText = dinoData.screen_name;
    e.appendChild(title);

    var sub = document.createElement('div');
    sub.className = "dino_entry_sub";
    sub.innerText = dinoData.classname;
    e.appendChild(sub);

    e.x_dino_data = dinoData;

    return e;
}

ark.latestDinoPickerSearch = null;
ark.searchDinoPicker = function(search, callback) {
    ark.latestDinoPickerSearch = search;
    //Devalidate the results box
    var box = document.getElementById('configure_dino_selector_results');
    box.classList.add("dino_search_content_load");

    //Web request
    ark.serverRequest(ark.session.endpoint_dino_class_search.replace("{query}",encodeURIComponent(search)), {}, function(d) {
        //Ensure this is the most recent requset
        if(d.query != ark.latestDinoPickerSearch) {
            return;
        }
        //Recreate results
        box.innerHTML = "";
        for(var i = 0; i<d.results.length; i+=1) {
            var data = d.results[i];
            var e = ark.createDinoClassEntry(data);
            e.addEventListener('click', function() {
                var dinoData = this.x_dino_data;
                callback(dinoData);
            });
            box.appendChild(e);
        }
        //Validate results box
        box.classList.remove("dino_search_content_load");
    });
}

ark.displayFullscreenText = function(text) {
    var d = document.createElement('div');
    d.className = "text_center";
    var de = document.createElement('p');
    de.innerText = text;
    d.appendChild(de);
    return ark.showCustomArea(d);
}

ark.displayActionableFullscreenText = function(text, actionText, actionCallback) {
    var d = document.createElement('div');
    d.className = "text_center";
    var de = document.createElement('p');
    de.innerText = text;
    d.appendChild(de);

    var btn = ark.createDom("div", "text_center_action", d);
    var btn_btn = ark.createDom("input", "", btn);
    btn_btn.type = "button";
    btn_btn.value = actionText;
    btn_btn.addEventListener('click', actionCallback);

    return ark.showCustomArea(d);
}

ark.showCustomArea = function(domContent) {
    ark.showNewCustomMenu(domContent, "");
}

ark.hideCustomArea = function() {
    ark.hideCustomMenu();
}

ark.createDom = function(type, classname, parent) {
    var e = document.createElement(type);
    e.className = classname;
    if(parent != null) {
        parent.appendChild(e);
    }
    return e;
}

ark.createForm = function(names, items, parent) {
    var tbl = ark.createDom("table", "np_form", parent);
    for(var i = 0; i<names.length; i+=1) {
        var h1 = ark.createDom("tr", "", tbl);
        if(typeof(names[i]) == "string") {
            ark.createDom("td", "np_form_title", h1).innerText = names[i];
        } else {
            ark.createDom("td", "np_form_title", h1).appendChild(names[i]);
        }
        ark.createDom("td", "", h1).appendChild(items[i]);
    }
    return tbl;
}

ark.createNumberWithCommas = function(data) {
    //https://stackoverflow.com/questions/2901102/how-to-print-a-number-with-commas-as-thousands-separators-in-javascript
    return Math.round(data).toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
}

ark.createCustomDinoEntry = function(img_url, title_text, sub_text, customClass) {
    //Create html node
    var e = document.createElement('div');
    e.className = "dino_entry "+customClass;

    var img = document.createElement('img');
    img.src = img_url;
    e.appendChild(img);

    var title = document.createElement('div');
    title.className = "dino_entry_title";
    title.innerText = title_text;
    e.appendChild(title);

    var sub = document.createElement('div');
    sub.className = "dino_entry_sub";
    sub.innerText = sub_text;
    e.appendChild(sub);

    return e;
}

//https://stackoverflow.com/questions/25500316/sort-a-dictionary-by-value-in-javascript
ark.sortDict = function(input) {
    // Create items array
    var items = Object.keys(input).map(function(key) {
        return [key, input[key]];
    });

    // Sort the array based on the second element
    items.sort(function(first, second) {
        return second[1] - first[1];
    });

    //Remap this
    var output = {};
    for(var i = 0; i<items.length; i+=1) {
        output[items[i][0]] = items[i][1];
    }

    return output;
}

ark.latestTribeItemSearchQuery = null;
ark.tribeItemSearchPage = 0;

ark.refreshTribeItemSearch = function(query) {
    ark.latestTribeItemSearchQuery = query;
    ark.tribeItemSearchPage = 0;
    
    //Get element
    var parent = document.getElementById('dino_search_window_content');

    ark.appendToDinoItemSearch(query, ark.tribeItemSearchPage, true);
}

ark.appendToDinoItemSearch = function(query, page, doClear) {
    //Deactivate
    var parent = document.getElementById('dino_search_window_content');
    parent.classList.add("sidebar_search_content_load");
    //Make a request
    
    ark.serverRequest(ark.session.endpoint_tribes_itemsearch.replace("{query}", query)+"&p="+page.toString(), {}, function(d) {
        //Check if this matches the latest
        if(d.query != ark.latestTribeItemSearchQuery) {
            return;
        }

        if(doClear) {
            parent.innerHTML = "";
        }

        //Create for each
        for(var i = 0; i<d.items.length; i+=1) {
            var r = d.items[i];

            //Create structure.
            var e = ark.createDom("div", "sidebar_item_search_entry", parent);
            var e_icon = ark.createDom("img", "sidebar_item_search_entry_icon", e);
            var e_title = ark.createDom("div", "sidebar_item_search_entry_text", e);
            var e_sub = ark.createDom("div", "sidebar_item_search_entry_sub", e);
            var e_dinos = ark.createDom("div", "sidebar_item_search_entry_dinos", e);

            //Set some values
            e_icon.src = r.item_icon;
            e_title.innerText = r.item_displayname;
            e_sub.innerText = ark.createNumberWithCommas(r.total_count)+" total";
            
            //Add all of the dinos.
            for(var j = 0; j< r.owner_inventories.length; j+=1) {
                var inventory = r.owner_inventories[j];
                var dino = d.owner_inventory_dino[inventory.id];
                
                var e_dom = (ark.createCustomDinoEntry(dino.img, dino.displayName, dino.displayClassName + " - x"+ark.createNumberWithCommas(inventory.count), "dino_entry_offset"));
                e_dom.x_dino_id = dino.id;
                e_dom.addEventListener('click', function() {
                    ark.locateDinoById(this.x_dino_id);
                });
                e_dinos.appendChild(e_dom);
            }

            
        }

        //Add "show more" if needed
        if(d.more) {
            var e = ark.createDom("div", "nb_button_blue", parent);
            e.style.marginTop = "20px";
            e.innerText = "Load More";
            e.addEventListener('click', function() {
                //Remove button and load more
                this.remove();
                ark.tribeItemSearchPage++;
                ark.appendToDinoItemSearch(ark.latestTribeItemSearchQuery, ark.tribeItemSearchPage, false);
            });
        }

        //Reactivate
        parent.classList.remove("sidebar_search_content_load");
    });
}

ark.locateDinoById = function(id) {
    //Locate on map.
    var mapPointer = map.dino_marker_list[id];
    var pin = mapPointer.map_icons[0];
    pin._map.flyTo(pin._latlng, 9, {
        "animate":true,
        "duration":0.5,
        "easeLinearity":0.25,
        "noMoveStart":false
    });
}

ark.getDinoUrl = function(id) {
    return ark.session.endpoint_tribes_dino.replace("{dino}", id);
}

ark.showNewCustomMenu = function(dom, customClasses) {
    //Set content
    var b = document.getElementById('new_custom_box');
    b.className = "new_custom_box "+customClasses;
    b.innerHTML = "";
    b.appendChild(dom);

    //Show
    document.getElementById('new_custom_area').classList.remove("new_custom_area_inactive");
    document.getElementById('main_view').classList.add("main_view_blurred");
}

ark.hideCustomMenu = function() {
    document.getElementById('new_custom_area').classList.add("new_custom_area_inactive");
    document.getElementById('main_view').classList.remove("main_view_blurred");
}

ark.deinitCurrentServer = function() {
    //Stop baby update loop
    bman.sessions = [];

    //Deinit map
    map.map.remove();

    //Restore default background color
    map.restoreDefaultBackgroundColor();

    //Kill heartbeat
    clearInterval(ark.session.eventLoop);

    //Collapse current dino
    map_menu.hide();

    //Clear baby dino list
    document.getElementById('dino_n_card_holder').innerHTML = "";

    //Hide dino search
    ark.hideSearchWindow();

    //Delete session data
    ark.session = null;

    //Show the template view
    ark.createTemplateView();
}

ark.setMainContentVis = function(active) {
    //Set message
    if(!active) {
        document.getElementById('no_session_mask').classList.remove("no_session_mask_disable");
    } else {
        document.getElementById('no_session_mask').classList.add("no_session_mask_disable");
    }
}


ark.onMouseOverSidebarServer = function() {
    //Show modal there.
    var e = document.createElement('div');
    e.style.position = "fixed";
    e.style.zIndex = 100;
    e.style.top = this.offsetTop - document.getElementById('server_list').scrollTop;
    e.style.left = "10px";

    //Set
    e.appendChild(this.e_modal);
    document.body.appendChild(e);
    this.open_modal = e;
}

ark.onEndMouseOverSidebarServer = function() {
    this.open_modal.remove();
}

ark.onHideServerButtonClick = function() {
    var d = ark.createDom("div", "");
    ark.createDom("div","nb_title nb_big_padding_bottom", d).innerText = "Hide Server?";
    ark.createDom("div","np_sub_title nb_big_padding_bottom", d).innerText = "Are you sure you would like to hide this server? You may unhide it at any time by clicking on the 'archive' button in the lower left of the screen."

    var b = ark.createDom("div", "nb_button_blue nb_button_back", d);
    b.innerText = "Cancel";
    b.addEventListener('click', function() {
        ark.hideCustomArea();
    });

    var bf = ark.createDom("div", "nb_button_blue nb_button_forward", d);
    bf.innerText = "Hide Server";
    bf.addEventListener('click', function() {
        ark.serverRequest("https://ark.romanport.com/api/users/@me/servers/add_ignore/?id="+ark.currentServerId, {}, function() {
            document.getElementById('server_badge_'+ark.currentServerId).remove();
            ark.hideCustomArea();
            ark.deinitCurrentServer();
        });
    });

    ark.showNewCustomMenu(d, "");
}

ark.currentSearchType = "tribeInventory";
ark.onStartSearchTyping = function(query) {
    //The user is typing into the search. Based on the type, decide what to do
    if(ark.currentSearchType == "tribeInventory") {
        ark.refreshTribeItemSearch(query);
    }
}

ark.showSearchWindow = function(type) {
    //Grab handles
    var p = document.getElementById('dino_search_window');
    var ps = document.getElementById('dino_search_window_content');

    //Clear
    ps.innerHTML = "";
    document.getElementById('dino_search_window_input').value = "";

    //Search
    ark.currentSearchType = type;
    ark.onStartSearchTyping("");

    //Show
    p.classList.remove('dino_search_window_disabled');
};

ark.hideSearchWindow = function() {
    //Hide
    var p = document.getElementById('dino_search_window');
    p.classList.add('dino_search_window_disabled');
}

/* Heatmap settings */
ark.chosen_filter_heatmap_dino = null;
ark.chosen_heatmap_pending_toggle_state = false; //State to set of the map.

ark.showHeatmapSettings = function() {
    var d = ark.createDom("div", "");
    ark.createDom("div","nb_title nb_big_padding_bottom", d).innerText = "Heatmap Settings";
    ark.createDom("div","np_sub_title nb_big_padding_bottom", d).innerText = "The heatmap shows general locations of wild dinos on the map."

    var toggleBtn = ark.createDom("div", "nb_button_blue");
    toggleBtn.innerText = "Turn On"
    if(ark.chosen_heatmap_pending_toggle_state == true) {
        toggleBtn.innerText = "Turn Off";
        toggleBtn.classList.add("nb_button_blue_inverted");
    }
    toggleBtn.style.width = "unset";
    toggleBtn.addEventListener('click', function() {
        ark.chosen_heatmap_pending_toggle_state = !ark.chosen_heatmap_pending_toggle_state;
        ark.showHeatmapSettings();
    });

    var searchArea;
    if(ark.chosen_filter_heatmap_dino == null) {
        //No dino chosen.
        searchArea = ark.createDom("div", "configureheatmap_dinoform");
        var searchAreaInput = ark.createDom("input", "dino_search_window_input", searchArea)
        searchAreaInput.type = "text";
        searchAreaInput.addEventListener('input', function() {
            ark.searchDinoPicker(searchAreaInput.value, ark.heatmapDinoEntryClicked);
        });
        var searchAreaOutput = ark.createDom("div", "dino_search_window_content", searchArea);
        searchAreaOutput.id = "configure_dino_selector_results";
    } else {
        searchArea = ark.createDinoClassEntry(ark.chosen_filter_heatmap_dino);
        searchArea.classList.add("configureheatmap_selected_dino_entry");
        searchArea.addEventListener('click', function() {
            //Reset dino data and show list
            ark.chosen_filter_heatmap_dino = null;
            ark.showHeatmapSettings();
        });
    }

    ark.createForm([
        "Is Shown",
        "Filter Dino Class"
    ], [
        toggleBtn,
        searchArea
    ], d);

    ark.createDom("div", "window_close_btn", d).addEventListener('click', function() {
        //Hide menu
        ark.hideCustomArea();

        //Apply settings.
        var classname = "";
        if(ark.chosen_filter_heatmap_dino != null) {
            classname = ark.chosen_filter_heatmap_dino.classname;
        }
        map.resetPopulationMap(ark.chosen_heatmap_pending_toggle_state, classname);
    });

    ark.showNewCustomMenu(d, "new_custom_box_tall");    

    if(ark.chosen_filter_heatmap_dino == null) {
        ark.searchDinoPicker("", ark.heatmapDinoEntryClicked);
    }
}

ark.heatmapDinoEntryClicked = function(dinoData) {
    //Set dino and reshow
    ark.chosen_filter_heatmap_dino = dinoData;
    ark.showHeatmapSettings();
}

ark.logoutBtnPressed = function() {
    var d = ark.createDom("div", "");
    ark.createDom("div","nb_title nb_big_padding_bottom", d).innerText = "Logout?";
    ark.createDom("div","np_sub_title nb_big_padding_bottom", d).innerText = "Are you sure you'd like to log out of ArkGameMap? You may sign in at any time using Steam to restore your servers."

    var b = ark.createDom("div", "nb_button_blue nb_button_back", d);
    b.innerText = "Cancel";
    b.addEventListener('click', function() {
        ark.hideCustomArea();
    });

    var bf = ark.createDom("div", "nb_button_blue nb_button_forward", d);
    bf.innerText = "Logout";
    bf.addEventListener('click', function() {
        ark.serverRequest("https://ark.romanport.com/api/users/@me/servers/logout", {}, function() {
            window.location.reload();
        });
    });

    ark.showNewCustomMenu(d, "");
};

ark.onClickOfflineServer = function(name, isOwner, id) {
    var d = ark.createDom("div", "");
    ark.createDom("div","nb_title nb_big_padding_bottom", d).innerText = "Server Offline";
    if(isOwner) {
        ark.createDom("div","np_sub_title nb_big_padding_bottom", d).innerText = "Your server, '"+name+"', is offline. Start the ArkWebMap service, or delete the server if you no longer use ArkWebMap. Deleting a server will remove the ability to access ArkWebMap from all users."
    } else {
        ark.createDom("div","np_sub_title nb_big_padding_bottom", d).innerText = "'"+name+"' is offline. Ask your server owner to start the ArkWebMap service to use it, or hide the server to remove it."
    }

    var bf = ark.createDom("div", "nb_button_blue nb_button_forward nb_button_red_color", d);
    if(isOwner) {
        bf.innerText = "Delete Server";
        bf.addEventListener('click', function() {
            ark.promptDeleteServer(id);
        });
    } else {
        bf.innerText = "Hide Server";
        bf.addEventListener('click', function() {
            ark.hideCustomArea();
        });
    }

    ark.createDom("div", "window_close_btn", d).addEventListener('click', function() {
        ark.hideCustomArea();
    });

    ark.showNewCustomMenu(d, "");
}

ark.promptDeleteServer = function(serverId) {
    var d = ark.createDom("div", "");
    ark.createDom("div","nb_title nb_big_padding_bottom", d).innerText = "Delete Server";
    ark.createDom("div","np_sub_title nb_big_padding_bottom", d).innerText = "You are about to delete this server. This action cannot be undone, however it can be restored by recompleting the ArkWebMap setup. This will not impact your Ark server, but will remove access to the ArkWebMap from all users."

    var bf = ark.createDom("div", "nb_button_blue nb_button_forward nb_button_red_color", d);
    bf.innerText = "Confirm Deletion";
    bf.addEventListener('click', function() {
        ark.serverRequest("https://ark.romanport.com/api/servers/"+serverId+"/delete", {"type":"post"}, function(d) {
            //Reload
            window.location.reload();
        });
        
    });

    ark.createDom("div", "window_close_btn", d).addEventListener('click', function() {
        ark.hideCustomArea();
    });

    ark.showNewCustomMenu(d, "");   
}

ark.onConnectedServerStop = function() {
    //Deinit
    ark.deinitCurrentServer();

    //Refresh user server list
    ark.refreshServers(function(){});

    //Show message
    var d = ark.createDom("div", "");
    ark.createDom("div","nb_title nb_big_padding_bottom", d).innerText = "Server Offline";
    ark.createDom("div","np_sub_title nb_big_padding_bottom", d).innerText = "The server you were connected to went offline. Try again later.";

    ark.createDom("div", "window_close_btn", d).addEventListener('click', function() {
        ark.hideCustomArea();
    });

    ark.showNewCustomMenu(d, "");
}

ark.convertTimeSpan = function(ms) {
    var output = {};
    var totalMs = ms;

    output.days = Math.floor(ms / (1000 * 60 * 60 * 24));
    ms -=  output.days * (1000 * 60 * 60 * 24);

    output.hours = Math.floor(ms / (1000 * 60 * 60));
    ms -= output.hours * (1000 * 60 * 60);

    output.mins = Math.floor(ms / (1000 * 60));
    ms -= output.mins * (1000 * 60);

    output.seconds = Math.floor(ms / (1000));
    ms -= output.seconds * (1000);

    output.totalMilliseconds = totalMs;
    output.totalSeconds = output.seconds + (output.mins * 60) + (output.hours * 60 * 60) + (output.days * 60 * 60 * 24);
    output.totalMinutes = (output.mins) + (output.hours * 60) + (output.days * 60 * 24);
    output.totalHours = (output.hours) + (output.days * 24);
    output.totalDays = output.days;

    return output;
}

ark.pluralToString = function(num, name) {
	if(num == 1) {
		return num+" "+name;
    } else {
		return num+" "+name+"s";
    }
}

ark.createLastUpdatedString = function(span) {
    //Check if it's been less than 1 minute
	if(span.totalMinutes == 0) {
		return "Last updated less than a minute ago";
    }
	
	//If it's under an hour, say the number of minutes ago it was updated only
	if(span.totalHours == 0) {
		return "Last updated "+ark.pluralToString(span.mins, "minute")+" ago";
    }

	//If it's under a day, say the number of hours and minutes
	if(span.totalDays == 0) {
		return "Last updated "+ark.pluralToString(span.hours, "hour")+", "+ark.pluralToString(span.mins, "minute")+" ago";
    }

	//Now, fallback to hours and days
	return "Last updated "+ark.pluralToString(span.days, "day")+", "+ark.pluralToString(span.hours, "hour")+" ago";
}

ark.updateLastEditedUi = function() {
    //Get the timespan
    var span = ark.convertTimeSpan(ark.getGameTimeOffset() * 1000);

    //Create string
    var stringName = ark.createLastUpdatedString(span);

    //Set in UI
    document.getElementById('map_sub_title').innerText = stringName;
}

ark.openDemoServer = function() {
    var demoServerId = "EzUn7Rab7e4BSM9JFrOsPpn0";
    ark.forceJoinServer("https://ark.romanport.com/api/servers/"+demoServerId+"/create_session", demoServerId, "ArkWebMap Demo");
}

ark.inflateMainMenu = function(data) {
    //Data is split into sections. Loop through
    var a = document.getElementById('sidebar_btns');
    a.innerHTML = "";
    for(var sectionId = 0; sectionId < data.length; sectionId+=1) {
        var section = data[sectionId];
        for(var i = 0; i<section.length; i+=1) {
            var item = section[i];
            var e = ark.createDom('div', 'sidebar_button '+item.customClass, a);
            e.innerText = item.name;
            var img = ark.createDom('img', '', e);
            img.src = item.img;
            e.addEventListener('click', item.callback);
        }
        if(sectionId != data.length - 1) {
            ark.createDom("div", "sidebar_button_spacer", a);
        }
    }
}

ark.inflateTemplateMainMenu = function(count) {
    //Data is split into sections. Loop through
    var a = document.getElementById('sidebar_btns');
    a.innerHTML = "";
    for(var i = 0; i<count; i+=1) {
        var e = ark.createDom('div', 'sidebar_button', a);
        e.appendChild(ark.generateTextTemplate(22, "#404144", 200));
        var img = ark.createDom('div', 'sidebar_button_templateimg', e);
    }
}

ark.createAndInflateMainMenu = function(session) {
    //Base menu
    var b = [
        [
            {
                "img":"/assets/icons/baseline-search-24px.svg",
                "name":"Search Inventories",
                "customClass":"",
                "callback":function() {
                    ark.showSearchWindow('tribeInventory');
                }
            }
        ],
        [

        ]
    ]

    //If permitted, add the heatmap options
    if(session.permissions.includes("allowHeatmap")) {
        b[0].push({
            "img":"/assets/icons/baseline-map-24px.svg",
            "name":"Heatmap Options",
            "customClass":"",
            "callback":function() {
                ark.showHeatmapSettings();
            }
        })
    }

    //Add server leave buttons
    if(session.isDemoServer) {
        b[1].push({
            "img":"/assets/icons/baseline-add_circle-24px.svg",
            "name":"Add Your Own Server",
            "customClass":"sidebar_button_accent",
            "callback":function() {
                create_server_d.onCreate();
            }
        });
    } else {
        b[1].push({
            "img":"/assets/icons/baseline-exit_to_app-24px.svg",
            "name":"Hide Server",
            "customClass":"sidebar_button_danger",
            "callback":function() {
                ark.onHideServerButtonClick();
            }
        });
    }

    //Inflate
    ark.inflateMainMenu(b);
}

ark.isInServerEditState = false;
ark.serverEditState = {};
ark.toggleServerNameEdit = function(isActive) {
    var ibox = document.getElementById('map_title');
    var ipbox = document.getElementById('image_picker_image');
    var button = document.getElementById('server_edit_button');
    ark.isInServerEditState = isActive;
    if(isActive) {
        ibox.classList.add("map_title_input_selected");
        ipbox.classList.add("server_edit_icon_active");
        ibox.removeAttribute("readonly");
        button.classList.add("server_edit_btn_save");

        //Set image
        ipbox.style.backgroundImage = "url(\""+ark.currentServer.image_url+"\")";

        //Reset state 
        ark.serverEditState = {};
    } else {
        ibox.classList.remove("map_title_input_selected");
        ipbox.classList.remove("server_edit_icon_active");
        ibox.setAttribute("readonly", "readonly");
        button.classList.remove("server_edit_btn_save");

        //Send data
        ark.serverEditState["name"] = ibox.value;

        //Send data
        ark.serverRequest("https://ark.romanport.com/api/servers/"+ark.currentServerId+"/edit", {"type":"post","body":JSON.stringify(ark.serverEditState)}, function(e) {
            console.log("Submitted new server settings");
        });
    }
}

ark.onImagePickerClick = function() {
    //Open file picker for image
    document.getElementById('image_picker').click();
}

ark.onImagePickerChooseImage = function() {
    console.log("Chose server image. Uploading...");

    //Create form data
    var formData = new FormData();
    formData.append("f", document.getElementById('image_picker').files[0]);

    //Send
    ark.serverRequest("https://user-content.romanport.com/upload?application_id=Pc2Pk44XevX6C42m6Xu3Ag6J", {
        "type":"post",
        "body":formData,
        "nocreds":true
    }, function(f) {
        //Update the image here
        var e = document.getElementById('image_picker_image');
        e.style.backgroundImage = "url('"+f.url+"')";
        ark.serverEditState["iconToken"] = f.token;
    });
}

ark.generateTextTemplate = function(fontHeight, color, maxWidth) {
    //Generate a random length
    var length = maxWidth * ((Math.random() * 0.5) + 0.25);
    var height = (fontHeight - 2);

    //Create element
    var e = ark.createDom("div", "glowing");
    e.style.width = length.toString()+"px";
    e.style.height = height.toString()+"px";
    e.style.marginTop = "1px";
    e.style.marginBottom = "1px";
    e.style.borderRadius = height.toString()+"px";
    e.style.backgroundColor = color;
    e.style.display = "inline-block";

    return e;
}

ark.createTemplateView = function() {
    //Create a view that is shown while we load data
    ark.inflateTemplateMainMenu(4);
    dinosidebar.createTemplate(20);

    //Show the template title and set content
    var title = document.getElementById('map_title_template');
    var title_content = title.firstElementChild;
    title_content.innerHTML = "";
    title_content.appendChild(ark.generateTextTemplate(18, "#4973c9", 270));
    title.style.display = "block";
    document.getElementById('map_title_main').style.display = "none";

    //Now, set the template of the sub title
    var sub = document.getElementById('map_sub_title');
    sub.innerHTML = "";
    sub.appendChild(ark.generateTextTemplate(15, "#565758", 270));
}

var create_server_d = {
    onCreate: function() {
        window.location = "/create";
    }
};