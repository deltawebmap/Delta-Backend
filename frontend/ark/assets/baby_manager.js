var bman = {};
bman.recent_dino = null;
bman.timer = null;

bman.sessions = [];
bman.playing_audio = [];

bman.addDinoTimer = function(d) {
    var embed = bman.generateEmbed(d);
    bman.sessions.push({
        "url":d.href,
        "embed":embed,
        "data":d
    });
    bman.recent_dino = d;

    //If the timeout is not set, set 
    if(bman.timer == null) {
        bman.timer = window.setInterval(bman.onTick, 1000);
    }

    return embed;
};

/* bman.addDinoTimer = function(d) {
    //Check if we already have this url
    for(var i = 0; i<bman.sessions.length; i+=1) {
        if(bman.sessions[i].url == url) {
            //While this does already exist, we can issue an update to it.
            ark.serverRequest(url, {"customErrorText":"Failed to download baby dino data."}, function(d) {
                bman.sessions[i].data = d;
                console.log("Updated dino url "+bman.sessions[i].url +" with data from "+url);
            });
            return;
        }
    }
    //Send request and begin.
    ark.serverRequest(url, {"customErrorText":"Failed to download baby dino data."}, function(d) {
        var embed = bman.generateEmbed(d);
        document.getElementById('dino_n_card_holder').appendChild(embed);
        bman.sessions.push({
            "url":url,
            "embed":embed,
            "data":d
        });
        bman.recent_dino = d;

        //If the timeout is not set, set 
        if(bman.timer == null) {
            bman.timer = window.setInterval(bman.onTick, 1000);
        }
    });
};*/

bman.removeAllSessions = function() {
    document.getElementById('dino_n_card_holder').innerHTML = "";
    bman.sessions = [];
}

bman.onTick = function() {
    bman.sendAlerts = true;
    for(var i = 0; i<bman.sessions.length; i+=1) {
        var embed = bman.generateEmbed(bman.sessions[i].data);
        bman.sessions[i].embed.replaceWith(embed);
        bman.sessions[i].embed = embed;
    }
}

bman.calculateCurrentDinoFood = function(dinoData) {
    //Calculates the current amount of food a baby dino has. dinoData = data from the dinos endpoint.
    var entryStatusComponent = dinoData.dino_entry.statusComponent;
    var dinoFoodLossPerSecond = entryStatusComponent.baseFoodConsumptionRate * entryStatusComponent.extraBabyDinoConsumingFoodRateMultiplier * entryStatusComponent.babyDinoConsumingFoodRateMultiplier * entryStatusComponent.foodConsumptionMultiplier;
    return dinoData.dino.currentStats.food + (dinoFoodLossPerSecond * ark.getGameTimeOffset());
};

bman.calculateTotalInventoryFood = function(dinoData) {
    //Get the food list of this dino class.
    var dinoFoodData = dinoData.dino_entry.childFoods;
    if(dinoFoodData == null) {
        //Fallback
        dinoFoodData = dinoData.dino_entry.adultFoods;
    }
    if(dinoFoodData == null) {
        //Throw error
        throw "No dino food info found";
    }

    //Calculates the total food energy inside the inventory of this dino.
    var total = 0;
    for(var i = 0; i < dinoData.inventory_items.length; i+=1) {
        //Loop through inventory items and get the total food energy.
        var item = dinoData.inventory_items[i];
        var item_data = dinoData.item_class_data[item.classnameString];

        //Check if this item data can give food
        if(item_data.addStatusValues["EPrimalCharacterStatusValue::Food"] != null) {
            var foodData = item_data.addStatusValues["EPrimalCharacterStatusValue::Food"];
            var foodEnergy = foodData.baseAmountToAdd * item.stackSize; //Get the total energy, multiplied by the stack size, but before calculating the dino's food data.
            
            //Check if we have data for this in the dino food data
            for(var j = 0; j<dinoFoodData.length; j+=1) {
                var foodData = dinoFoodData[i];
                if(foodData.classname == item.classnameString) {
                    //Great. We got the info. Add it and break.
                    total += foodData.foodEffectivenessMultiplier * foodEnergy;
                    break;
                }
            }
        }
    }

    return total;
}

bman.calculateTotalDinoFood = function(dinoData) {
    //Calculates the total, including inventory food and current stats
    return bman.calculateCurrentDinoFood(dinoData) + bman.calculateTotalInventoryFood(dinoData);
}

bman.calculateFoodLossPerSecond = function(dinoData) {
    //Return dino food loss per second
    var entryStatusComponent = dinoData.dino_entry.statusComponent;
    return dinoFoodLossPerSecond = entryStatusComponent.baseFoodConsumptionRate * entryStatusComponent.extraBabyDinoConsumingFoodRateMultiplier * entryStatusComponent.babyDinoConsumingFoodRateMultiplier * entryStatusComponent.foodConsumptionMultiplier;
}

bman.calculateTimeToFoodDepletionMs = function(foodAmount, dinoData) {
    //Calculate the amount of time until this food is gone, in ms.
    var dinoFoodLossPerSecond = bman.calculateFoodLossPerSecond(dinoData);
    return (foodAmount / Math.abs(dinoFoodLossPerSecond)) * 1000;
}

bman.padNum = function(e) {
    return e.toString().padStart(2, "0");
}

bman.createTimeString = function(diff) {
    if(diff < 0) {
        return "-"+bman.createTimeString(Math.abs(diff));
    }

    var span = ark.convertTimeSpan(diff);

    //Do conversions
    var baseString = bman.padNum(span.hours)+":"+bman.padNum(span.mins)+":"+bman.padNum(span.seconds);
    if(span.days != 0) {
        baseString = bman.padNum(span.days)+":"+baseString;
    }
    return baseString;
}

bman.calculateFullBabyInfo = function(dinoData) {
    //Calculate
    var currentFood = bman.calculateCurrentDinoFood(dinoData);
    var inventoryFood = bman.calculateTotalInventoryFood(dinoData);
    var totalCurrentFood = currentFood + inventoryFood;
    var foodLossPerSecond = bman.calculateFoodLossPerSecond(dinoData);
    var timeToDepletionMs = bman.calculateTimeToFoodDepletionMs(totalCurrentFood, dinoData); 
    var timeToDepletionString = bman.createTimeString(timeToDepletionMs);

    //Create single object
    return {
        "currentFood":currentFood,
        "inventoryFood":inventoryFood,
        "totalCurrentFood":totalCurrentFood,
        "foodLossPerSecond":foodLossPerSecond,
        "timeToDepletionMs":timeToDepletionMs,
        "timeToDepletionString":timeToDepletionString
    };
}

bman.sendAlerts = true;
bman.tempMute = false;

bman.generateEmbed = function(dinoData) {
    //Get data
    var babyInfo = bman.calculateFullBabyInfo(dinoData);
    var dinoAge = dinoData.dino.babyAge;
    var timeUntilImprintingMs = (dinoData.dino.nextImprintTime -ark.getCurrentGameTime())  * 1000;
    var timeUntilImprintString = "Unknown next imprint";
    if(dinoData.dino.nextImprintTime == 0) {
        timeUntilImprintString = "Unknown next imprint";
    } else if(timeUntilImprintingMs < 0) {
        timeUntilImprintString = "Imprint ready!";
    } else {
        timeUntilImprintString = bman.createTimeString(timeUntilImprintingMs)+" until next";
    }

    //Create embed.
    var card = ark.createDom("div", "dino_n_card");

    var title = ark.createDom("div", "dino_n_card_title", card);
    title.innerText = dinoData.dino.tamedName+" ";
    
    //var title_sub = ark.createDom("span", "", title);
    //title_sub.innerText = "("+dinoData.dino_entry.screen_name+", Lvl "+dinoData.dino.baseLevel+")";

    var form = ark.createDom("div", "dino_n_card_form", card);
    var table = ark.createDom("table", "", form);
    table.appendChild(bman.private_generateFormEntry("Time Until Depleted", babyInfo.timeToDepletionString));
    table.appendChild(bman.private_generateFormEntry("Food Remaining", (Math.round(babyInfo.totalCurrentFood)).toString()));

    card.appendChild(bman.private_createBar("Imprinting", timeUntilImprintString, dinoData.dino.imprintingPercent * 100, 45));
    card.appendChild(bman.private_createBar("Maturing", (Math.round(dinoAge * 100)).toString()+"%", dinoAge * 100, 8));

    //Sound alerts if needed
    if(bman.sendAlerts) {
        if(babyInfo.timeToDepletionMs < 0) {
            //Critical!
            if(!bman.tempMute) {
                bman.playAlert(2, dinoData.dino.tamedName+" is starving and needs food immediately.");
            }
            bman.sendAlerts = false;
        }
        if(babyInfo.timeToDepletionMs < 10*60*1000) {
            //Warning
            if(!bman.tempMute) {
                bman.playAlert(2, dinoData.dino.tamedName+" needs food soon.");
            }
            bman.sendAlerts = false;
        }

        if(bman.sendAlerts == true) {
            //Did not get flagged. Unmute
            bman.tempMute = false;
        }
    }
    
    

    return card;
}

bman.private_generateFormEntry = function(h1, h2) {
    var tr = ark.createDom("tr", "");
    var t1 = ark.createDom("td", "", tr);
    var t2 = ark.createDom("td", "dino_n_card_form_value", tr);
    t1.innerText = h1;
    t2.innerText = h2;
    return tr;
}

bman.private_createBar = function(h1, h2, percent, bottom) {
    var container = ark.createDom("div", "dino_n_card_bar_container");
    container.style.bottom = bottom.toString()+"px";
    container.innerText = h1;
    var sub = ark.createDom("span", "", container);
    sub.innerText = h2;
    var bg = ark.createDom("div", "dino_n_card_bar_bg", container);
    var bar = ark.createDom("div", "dino_n_card_bar", bg);
    bar.style.width = percent.toString()+"%";

    return container;
}

bman.triggered_uids = {

};

bman.triggerAlarm = function(alarmid, uid, title, sub) {
    //Find id
    var trigger_sound = true;
    if(bman.triggered_uids[uid] != null) {
        //Do not trigger sound. Do not trigger text if the alarmid is less than the current one.
        trigger_sound = false;

        //Check priority
        if(bman.triggered_uids[uid] < alarmid) {
            return;
        }
    }

    //Show text.
    var d = ark.createDom("div", "");
    ark.createDom("div","nb_title nb_big_padding_bottom", d).innerText = "Dino Alert";
    ark.createDom("div","np_sub_title nb_big_padding_bottom", d).innerText = "Blue is starving and needs food now."
    var bf = ark.createDom("div", "nb_button_blue nb_button_forward", d);
    bf.innerText = "Mute";
    bf.addEventListener('click', function() {
        
    });
    ark.showNewCustomMenu(d, "");

    //Trigger sound

    //Set value
    bman.triggered_uids[uid] = alarmid;
}

bman.audio_urls = [
    "https://ark.romanport.com/assets/dino_food_low.wav",
    "https://ark.romanport.com/assets/dino_imprint.wav",
    "https://ark.romanport.com/assets/dino_food_critical.wav",
    "https://ark.romanport.com/assets/dino_disconnect.wav",
];

bman.current_audio = null;
bman.current_audio_priority = null;

bman.playAlert = function(alertId, message) {
    console.warn("Alerts have been disabled for now.");
    return;
    //0: Food warn
    //2: Food crit
    //1: Imprint
    //3: Disconnect

    //Check if 
    if(bman.current_audio != null) {
        //If the priority of this is greater, replace
        if(alertId > bman.current_audio_priority) {
            //Stop
            bman.current_audio.pause();
            bman.current_audio_priority = alertId;
            bman.current_audio = null;
        } else {
            //Ignore
            return;
        }
    }

    //Start
    bman.current_audio = new Audio(bman.audio_urls[alertId]);
    window.setTimeout(function() {
        bman.current_audio.play();
    }, 1000);
    bman.current_audio.loop = true;

    //Show alert
    ark.displayActionableFullscreenText(message, "Dismiss", function() {
        ark.hideCustomArea();
        bman.tempMute = true;
        bman.stopAlert();
    });
}

bman.stopAlert = function() {
    bman.current_audio.pause();
    bman.current_audio = null;
}