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
}

ark.onEvent_MapUpdate = function(d, time) {
    console.log("GOT MAP UPDATE EVENT");

    //Reload dino locations
    map.onEnableTribeDinos();
}

ark.eventDict = {
    0: ark.onEvent_MapUpdate
};

ark.switchServer = function(serverData) {
    //If a session is already open, kill
    if(ark.session != null) {
        ark.deinitCurrentServer();
    }
    ark.currentServerId = serverData.id;

    //First, open a session.
    bottom_modal.showLoaderBottom("Loading ARK map...", function(e) {
        ark.openSession(function(session) {
            //Kill window
            ark.hideCustomArea();
            
            //Init the map
            map.init(session.endpoint_game_map);

            //Fetch tribes
            map.onEnableTribeDinos();
    
            //Clear out the tribe item search
            ark.refreshTribeItemSearch("");
            
            //Fill UI
            document.getElementById('map_title').innerText = serverData.display_name;
            //document.getElementById('map_title_2').innerText = session.mapName;
            //document.getElementById('game_time').innerText = session.dayTime;

            //Show
            ark.setMainContentVis(true);
    
            //Done
            e();
            document.getElementById('no_session_mask').classList.add("no_session_mask_disable");
        }, serverData.endpoint_createsession);
    }, "bottom_modal_good", function(){});
    
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
            e.addEventListener('click', function() {
                ark.switchServer(this.x_server_data);
            });
            e.addEventListener('mouseover', ark.onMouseOverSidebarServer);
            e.addEventListener('mouseout', ark.onEndMouseOverSidebarServer);

            //Start pinging
            ark.serverRequest(s.endpoint_ping, {}, function(ping) {
                var ele = document.getElementById("server_badge_"+ping.id).e_modal;
                if(ping.online) {
                    //Show ping
                    ele.firstChild.innerText = ping.display_name+" ("+Math.round(ping.ping).toString()+" ms)";
                } else {
                    //Set to offline icon
                    ele.firstChild.innerText = ping.display_name + " (Server Offline)";
                    ele.style.backgroundImage = "url(/assets/server_offline.png), url("+s.image_url+")";
                    ele.firstChild.classList.add("badge_errbg");
                    ele.lastChild.classList.add("badge_errbg_triangle");
                }
            });
        }

        //Call callback
        callback(user);
    });
}

ark.init = function() {
    //Update user content
    ark.refreshServers(function(user) {
        console.log(user);

        //Set the icon image
        document.getElementById('my_badge').style.backgroundImage = "url("+user.profile_image_url+")";
    });
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
    ark.serverRequest("https://ark.romanport.com/api/servers/TFLEI5S8GMxHdZzpRrWbvtf3/dino_search/?query="+encodeURIComponent(search), {}, function(d) {
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
        ark.createDom("td", "np_form_title", h1).innerText = names[i];
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
        for(var i = 0; i<d.results.length; i+=1) {
            var r = d.results[i];

            //Create structure.
            var e = ark.createDom("div", "sidebar_item_search_entry", parent);
            var e_icon = ark.createDom("img", "sidebar_item_search_entry_icon", e);
            var e_title = ark.createDom("div", "sidebar_item_search_entry_text", e);
            var e_sub = ark.createDom("div", "sidebar_item_search_entry_sub", e);
            var e_dinos = ark.createDom("div", "sidebar_item_search_entry_dinos", e);

            //Set some values
            e_icon.src = "https://ark.romanport.com/resources/missing_icon.png";
            if(r.entry.icon != null) {
                e_icon.src = r.entry.icon.icon_url;
            }
            e_title.innerText = r.entry.name;
            e_sub.innerText = ark.createNumberWithCommas(r.total_count)+" total";

            //Sort
            r.owner_ids = ark.sortDict(r.owner_ids);
            
            //Add all of the dinos.
            var keys = Object.keys(r.owner_ids);
            for(var j = 0; j< keys.length; j+=1) {
                var jd = r.owner_dinos[keys[j]];
                var e_dom;
                if(jd.entry == null) {
                    e_dom = (ark.createCustomDinoEntry("https://ark.romanport.com/resources/missing_icon.png", jd.tamedName, jd.classname + " - x"+ark.createNumberWithCommas(r.owner_ids[keys[j]]), "dino_entry_offset"));
                } else {
                    e_dom = (ark.createCustomDinoEntry(jd.entry.icon_url, jd.tamedName, jd.entry.screen_name + " - x"+ark.createNumberWithCommas(r.owner_ids[keys[j]]), "dino_entry_offset"));
                }
                e_dom.x_dino_id = jd.id;
                e_dom.addEventListener('click', function() {
                    ark.locateDinoById(this.x_dino_id);
                });
                e_dinos.appendChild(e_dom);
            }

            
        }

        //Add "show more" if needed
        if(d.moreListItems) {
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

    //Hide
    document.getElementById('no_session_mask').classList.remove("no_session_mask_disable");
    ark.setMainContentVis(false);
}

ark.mainContentIds = [
    "sidebar_map",
    "map_part"
]

ark.setMainContentVis = function(active) {
    for(var i = 0; i<ark.mainContentIds.length; i+=1) {
        var d = document.getElementById(ark.mainContentIds[i]);
        if(active){
            d.classList.remove("hidden");
        } else {
            d.classList.add("hidden");
        }
    }

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