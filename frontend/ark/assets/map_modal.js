var map_menu = {};
map_menu.e = document.getElementById('bottom_modal');

map_menu.dino = null;
map_menu.dino_inventory = null;
map_menu.dino_entry = null;
map_menu.dino_max_stats = null;
map_menu.inventory_entries = null;

map_menu.isClosing = false;
map_menu.isOpening = false;
map_menu.isOpen = false;

map_menu.hide = function() {
    if(map_menu.isOpen == false) {
        return false;
    }
    map_menu.isClosing = true;
    collap.setState("dino_modal", false);

    window.setTimeout(function() {
        map_menu.isOpen = false;
        map_menu.isClosing = false;
    }, 300);
}

map_menu.show = function(url) {
    //First, set data
    map_menu.isOpening = true;
    var dinoLoadMessageTimeout = window.setTimeout(function() {
        ark.displayFullscreenText("Dino data is taking longer than usual to download. Please wait...");
        dinoLoadMessageTimeout = null;
    }, 1800);
    ark.serverRequest(url, {"customErrorText":"Failed to download dino data."}, function(d) {
        //If another window is currently closing, wait
        var timeout = 0;
        if(map_menu.isClosing) {
            timeout = 310;
        }

        //If the loading message is up, close it. If not, cancel it
        if(dinoLoadMessageTimeout == null) {
            ark.hideCustomArea();
        } else {
            clearTimeout(dinoLoadMessageTimeout);
        }

        window.setTimeout(function() {
            //Set vars
            map_menu.dino = d.dino;
            map_menu.dino_inventory = d.inventory_items;
            map_menu.dino_entry = d.dino_entry;
            map_menu.dino_max_stats = d.max_stats;
            map_menu.inventory_entries = d.item_class_data;

            //Show
            //Create UI
            //Set title
            document.getElementById('bottom_modal_bar').innerText = map_menu.dino.tamedName + " ("+map_menu.dino_entry.screen_name+")";

            //Set color area
            document.getElementById('bottom_modal_bar').appendChild(map_menu.setColorArea());
            
            //Set image
            document.getElementById('bottom_modal_dino_icon').src = map_menu.dino_entry.icon_url;

            //Add statuses
            var statusesToUse = [
                "health",
                "stamina",
                "inventoryWeight",
                "food"
            ];
            var statsContainer = document.getElementById('stats_column');
            statsContainer.innerHTML = "";
            for(var i = 0; i<statusesToUse.length; i+=1) {
                statsContainer.appendChild(map_menu.createStatusDom(statusesToUse[i]));
            }

            //Add items
            var itemsContainer = document.getElementById('inventory_area');
            itemsContainer.innerHTML = "";
            for(var i = 0; i < map_menu.dino_inventory.length; i+=1) {
                if(map_menu.dino_inventory[i].isEngram == false) {
                    itemsContainer.appendChild(map_menu.createItemDom(map_menu.dino_inventory[i]));
                }
            }

            //Force resize
            map_menu.resizeMenu();

            //Next, activate modal and deactivate map
            collap.setState("dino_modal", true);

            window.setTimeout(function() {
                map_menu.isOpen = true;
                map_menu.isOpening = false;
            }, 300);
        }, timeout);
    });
};

map_menu.setColorArea = function() {
    //Set the colors area.
    var area = document.createElement('div');
    area.className = "bottom_modal_color_area";

    //Loop through the dino colors and set them.
    var colors = map_menu.dino.colors_hex;
    var colorCount = colors.length;
    var colorCellPercent = 1 / colorCount;
    for(var i = 0; i<colors.length; i+=1) {
        var color = colors[i];
        var node = document.createElement('div');
        node.className = "bottom_modal_color_area_node";
        var node_inner = document.createElement('div');
        node_inner.className = "bottom_modal_color_area_node_inner";
        
        //Get percentage down
        var cellPercent = colorCellPercent * i;
        node.style.left = (cellPercent * 100).toString() + "%";
        node.style.width = (colorCellPercent * 100).toString() + "%";

        //Set color
        node_inner.style.backgroundColor = "#"+color;

        //Add
        area.appendChild(node);
        node.appendChild(node_inner);
    }
    return area;
}

map_menu.createStatusDom = function(statName) {
    //Get the status entry and dino data
    var statusEntry = ark.statusEntries[statName];
    var maxStat = map_menu.dino_max_stats[statName];
    var currentStat = map_menu.dino.currentStats[statName];

    //Create a DOM element
    var e = ark.createDom('div', 'stats_item');
    var bar = ark.createDom('div', 'stats_item_bar', e);
    var content = ark.createDom('div', 'stats_item_content', e);
    var img = ark.createDom('img', '', content);
    var title = ark.createDom('div', 'title', content);
    var amount = ark.createDom('div', 'amount', content);

    //Set img and title
    img.src = statusEntry.icon;
    title.innerText = statusEntry.name;

    //Set bar
    var percent = (currentStat / maxStat) * 100;
    if(percent > 100) {percent = 100;}
    bar.style.width = percent.toString()+"%";

    //Set amount
    amount.innerText = ark.createNumberWithCommas(currentStat) + " / "+ark.createNumberWithCommas(maxStat);

    return e;
}

map_menu.createItemDom = function(itemData) {
    //Grab item entry
    var itemEntry = map_menu.inventory_entries[itemData.classnameString];

    //Create dom
    var e = ark.createDom('div', 'inventory_cell');
    var icon = ark.createDom('img', 'inventory_cell_icon', e);
    var amount = ark.createDom('div', 'inventory_cell_amount', e);
    var weight = ark.createDom('div', 'inventory_cell_weight', e);
    var popup = ark.createDom('div', 'inventory_cell_mouseover', e);
    var popup_content = ark.createDom('span', 'inventory_cell_mouseover_content', popup);

    if(itemEntry == null) {
        //Fill data
        icon.src = "https://ark.romanport.com/resources/missing_icon.png"
        popup_content.innerText = itemData.classnameString + " (Item entry not found!)";
        amount.innerText = "x"+itemData.stackSize.toString();
        weight.innerText = "Missing Entry";
    } else {
        //Fill data
        icon.src = itemEntry.icon.icon_url;
        popup_content.innerText = itemEntry.name;
        amount.innerText = "x"+itemData.stackSize.toString();
        var stackWeightRounded = Math.round(itemEntry.baseItemWeight * itemData.stackSize * 10) / 10;
        var name = stackWeightRounded.toString();
        if(stackWeightRounded % 1 == 0) {
            name += ".0";
        }
        weight.innerText = name;
    }

    

    return e;
}

map_menu.resizeMenu = function() {
    //First, resize inventory
    var inventorySpace = window.innerWidth - 400 - 40 - 300;
    var inventorySlots = Math.floor(inventorySpace / 100);
    if(inventorySlots<1) {
        inventorySlots = 1;
    }
    document.getElementById('inventory_area').style.width = ((inventorySlots * 101)+20).toString() + "px";
}

//Register resize
window.addEventListener("resize", map_menu.resizeMenu);