var dinosidebar = {};

dinosidebar.fetchAndGenerate = function(callback) {
    ark.serverRequest(ark.session.endpoint_tribes_overview, {}, function(d) {
        dinosidebar.generate(d);
        callback();
    });
}

dinosidebar.generate = function(webData) {
    //Create a request for the create command
    var req = [
        {
            "type":"custom",
            "name":"Baby Dinos",
            "data":webData.baby_dinos,
            "onInflate":function(d) {
                return bman.addDinoTimer(d);
            },
            "onClick":function() {

            }
        },
        {
            "type":"auto",
            "name":"Tribemates",
            "data":webData.tribemates,
            "onClick":function() {
                //Open Steam URL
                window.open(this.x_data.steamUrl, "_blank");
            },
            "subKeyName":"arkName",
            "imageKeyName":"img",
            "nameKeyName":"steamName",
            "doInvertImage":false
        },
        {
            "type":"auto",
            "name":"Tribe Dinos",
            "data":webData.dinos,
            "onClick":function() {
                //Show dino on the map
                ark.locateDinoById(this.x_data.id);
            },
            "subKeyName":"classDisplayName",
            "imageKeyName":"img",
            "nameKeyName":"displayName",
            "additionalSubName":"level",
            "additionalSubDisplayName":"Lvl",
            "doInvertImage":true
        }
    ]
    dinosidebar.create(req);
}

dinosidebar.create = function(data) {
    var a = document.getElementById('dino_sidebar');
    a.innerHTML = "";
    for(var sid = 0; sid < data.length; sid += 1) {
        var section = data[sid];
        if(section.data.length == 0) {
            continue;
        }
        //Section data: type, data, name
        //If type == 'auto', use onClick, subKeyName, nameKeyName, imageKeyName, additionalSubName, additionalSubDisplayName, doInvertImage
        //If type == 'custom', have onInflate, onClick
        if(sid != 0) {
            var title = ark.createDom('div', 'dino_sidebar_section_header', a);
        }
        //title.innerText = section.name;
        for(var i = 0; i<section.data.length; i+=1) {
            var d = section.data[i];
            if(section.type == 'auto') {
                var e = ark.createDom('div', 'dino_sidebar_item', a);
                var img;
                if(section.doInvertImage) {
                    img = ark.createDom('img', 'dino_sidebar_item_invertedimg', e);
                } else {
                    img = ark.createDom('img', '', e);
                }
                var name = ark.createDom('div', 'dino_sidebar_item_title', e);
                var sub = ark.createDom('div', 'dino_sidebar_item_sub', e);
                img.src = d[section.imageKeyName];
                name.innerText = d[section.nameKeyName];
                sub.innerText = d[section.subKeyName];
                if(section.additionalSubName != null) {
                    //Append to sub
                    sub.innerText += " - "+section.additionalSubDisplayName+" "+d[section.additionalSubName];
                }
                e.x_data = d;
                e.addEventListener('click', section.onClick);
            } else if (section.type == 'custom') {
                //Make it inflate itself
                var e = section.onInflate(d);
                e.addEventListener('click', section.onClick);
                a.appendChild(e);
            } else {
                //Unknown
                throw "Unknown type.";
            }
        }
    }
}

dinosidebar.createTemplate = function(count) {
    var a = document.getElementById('dino_sidebar');
    a.innerHTML = "";
    for(var i = 0; i<count; i+=1) {
        var e = ark.createDom('div', 'dino_sidebar_item', a);
        var img = ark.createDom('div', 'dino_sidebar_item_templateimg', e);
        var name = ark.createDom('div', 'dino_sidebar_item_title', e);
        var sub = ark.createDom('div', 'dino_sidebar_item_sub', e);
        
        //Fill with templates
        name.appendChild(ark.generateTextTemplate(16, "#404144", 250));
        sub.appendChild(ark.generateTextTemplate(12, "#37383a", 150));
    }
}