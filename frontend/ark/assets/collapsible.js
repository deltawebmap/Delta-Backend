var collap = {};

collap.items = {
    "right_sidebar": {
        "is_open":true,
        "element":document.getElementById('dino_sidebar_container'),
        "open_class":"dino_sidebar_open",
        "effected_elements":[
            {
                "element":document.getElementById('map_part'),
                "open_class":"map_part_active_rightsidebar"
            },
            {
                "element":document.getElementById('bottom_modal'),
                "open_class":"map_part_active_rightsidebar"
            }
        ]
    },
    "left_sidebar": {
        "is_open":true,
        "element":document.getElementById('sidebar_part'),
        "open_class":"sidebar_part_active",
        "effected_elements":[
            {
                "element":document.getElementById('map_part'),
                "open_class":"map_part_active_leftsidebar"
            },
            {
                "element":document.getElementById('bottom_modal'),
                "open_class":"map_part_active_leftsidebar"
            }
        ]
    },
    "dino_modal": {
        "is_open":false,
        "element":document.getElementById('bottom_modal'),
        "open_class":"bottom_modal_active",
        "effected_elements":[
            {
                "element":document.getElementById('map_part'),
                "open_class":"map_modal"
            }
        ]
    }
}

collap.setOrRemoveClassname = function(ele, className, isActive) {
    if(isActive) {
        ele.classList.add(className);
    } else {
        ele.classList.remove(className);
    }
}

collap.setState = function(tagName, isOpen) {
    //Get the item
    var item = collap.items[tagName];

    //Apply or remove open class
    collap.setOrRemoveClassname(item.element, item.open_class, isOpen);

    //Apply to items effected by this
    for(var i = 0; i<item.effected_elements.length; i+=1) {
        var effect = item.effected_elements[i];

        //Apply or remove open class
        collap.setOrRemoveClassname(effect.element, effect.open_class, isOpen);
    }

    //Set state
    collap.items[tagName].is_open = isOpen;
}

collap.toggle = function(tagName) {
    //Set state
    collap.setState(tagName, !collap.items[tagName].is_open);
}