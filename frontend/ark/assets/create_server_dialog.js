var create_server_d = {};

create_server_d.permissions_list = [
    {
        "internal_name":"allowViewTamedTribeDinoStats",
        "display_name":"Allow Viewsing Tamed Dino Stats",
        "description":"Allows a user to view the stats or inventory of a tamed tribe dino.",
        "default":true
    },
    {
        "internal_name":"allowSearchTamedTribeDinoInventories",
        "display_name":"Allow Tribe Inventory Search",
        "description":"Allows a user to search all inventories in their tribe for items.",
        "default":true
    },
    {
        "internal_name":"allowHeatmap",
        "display_name":"Allow Heatmap",
        "description":"Allows a user to view the heatmap of wild dinos.",
        "default":true
    },
    {
        "internal_name":"allowHeatmapDinoFilter",
        "display_name":"Allow Heatmap Filter",
        "description":"Allows a user to filter the heatmap to only show specific dinos",
        "default":true
    },
]

create_server_d.steps = [
    function(id) {
        var d = ark.createDom("div", "");
        ark.createDom("div","nb_title nb_big_padding_bottom", d).innerText = "Create a New Server";
        ark.createDom("div","np_sub_title nb_big_padding_bottom", d).innerText = "Welcome! To get ArkWebMap set up on your Ark server, you'll need to download the companion program. This program, written by me, reads your Ark save and serves a web server to serve save data on the web map."
        ark.createDom("div","nb_button_blue nb_big_padding_bottom nb_giant_top", d).innerText = "Download for Windows";
        ark.createDom("div","nb_button_blue nb_big_padding_bottom", d).innerText = "Download for Linux";
        var a = ark.createDom("a", "", ark.createDom("div","np_sub_title nb_title_center nb_link_container", d))
        a.innerText = "VirusTotal Scan (Windows)";
        a.href = "https://virustotal.com/"

        var b = ark.createDom("div", "nb_button_blue nb_button_back", d);
        b.innerText = "Abort";
        b.addEventListener('click', create_server_d.onBackButtonPressed);

        var bf = ark.createDom("div", "nb_button_blue nb_button_forward", d);
        bf.innerText = "Next Step";
        bf.addEventListener('click', create_server_d.onNextButtonPressed);

        ark.showNewCustomMenu(d, "new_custom_box_tall");
    },
    function(id) {
        var d = ark.createDom("div", "");
        ark.createDom("div","nb_title nb_big_padding_bottom", d).innerText = "Preparing Server";
        ark.createDom("div","np_sub_title nb_big_padding_bottom", d).innerText = "Run the companion program downloaded. The first time you open it, you'll be prompted for a code. Please type the code below into the program when prompted."
        ark.createDom("div","nb_setup_big_auth_code nb_title_center nb_big_padding_bottom", d).innerText = id;
        ark.createDom("div","np_sub_title nb_big_padding_bottom", d).innerText = "Once you type this code into the program and hit enter, the next step will automatically be displayed."

        var b = ark.createDom("div", "nb_button_blue nb_button_back", d);
        b.innerText = "Back";
        b.addEventListener('click', create_server_d.onBackButtonPressed);

        ark.showNewCustomMenu(d, "new_custom_box_tall");
    },
    function(id) {
        var d = ark.createDom("div", "");
        ark.createDom("div","nb_title nb_big_padding_bottom", d).innerText = "Configuring Server";
        ark.createDom("div","np_sub_title nb_big_padding_bottom", d).innerText = "Connected to the server! We'll need to do some configuration first.";
        ark.createDom("div","np_sub_title nb_big_padding_bottom", d).innerText = "The ArkWebMap hosts a web server that is only used to exchange map data with my own server. You'll need to forward a TCP port for this web server, much like you did with Ark.";

        var port_box = ark.createDom("div","nb_setup_big_auth_code nb_title_center nb_big_padding_bottom", d);
        var port_box_input = ark.createDom("input","nb_transparent_input", port_box);
        port_box_input.value = create_server_d.currentPort;
        port_box_input.id = "port_box_input";
        var port_box_accept = ark.createDom("div","nb_button_blue nb_big_padding_bottom", port_box);
        port_box_accept.innerText = "Verify";
        port_box_accept.addEventListener('click', create_server_d.onVerifyPortButtonPressed);

        ark.createDom("div","np_sub_title nb_big_padding_bottom", d).innerText = "The default port chosen is close to the Ark server, but you may choose a different one if you need to do so. Click \"Verify\" once you're done forwarding this port."

        var b = ark.createDom("div", "nb_button_blue nb_button_back", d);
        b.innerText = "Abort";
        b.addEventListener('click', create_server_d.onEnd);

        ark.showNewCustomMenu(d, "new_custom_box_tall");
    },
    function(id) {
        var d = ark.createDom("div", "");
        ark.createDom("div","nb_title nb_big_padding_bottom", d).innerText = "Testing Server Failed";
        ark.createDom("div","np_sub_title nb_big_padding_bottom", d).innerText = "Server test failed! Make sure the port you chose wasn't already used and that it is forwarded correctly.";

        var b = ark.createDom("div", "nb_button_blue nb_button_back", d);
        b.innerText = "Back";
        b.addEventListener('click', create_server_d.onBackButtonPressed);

        ark.showNewCustomMenu(d, "new_custom_box_tall");
    }, 
    function(id) {
        var d = ark.createDom("div", "");
        ark.createDom("div","nb_title nb_big_padding_bottom", d).innerText = "Testing Server Passed";
        ark.createDom("div","np_sub_title nb_big_padding_bottom", d).innerText = "Server test passed! Your server will run on this port.";

        var bf = ark.createDom("div", "nb_button_blue nb_button_forward", d);
        bf.innerText = "Continue";
        bf.addEventListener('click', create_server_d.onNextButtonPressed);

        ark.showNewCustomMenu(d, "new_custom_box_tall");
    },
    function(id) {
        var d = ark.createDom("div", "");
        ark.createDom("div","nb_title nb_big_padding_bottom", d).innerText = "Locate Ark Map";
        ark.createDom("div","np_sub_title nb_big_padding_bottom", d).innerText = "You must now locate your ARK file. It lives in the {ark}/ShooterGame/Saved/SavedArks directory by default. The file should end in .ark and begin with the name of the map you are playing on.";

        var entry = ark.createDom("input","nb_input_text", d);
        entry.id="nb_map_save_entry";
        entry.value = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\ARK\\ShooterGame\\Saved\\SavedArks\\Extinction.ark";

        var bf = ark.createDom("div", "nb_button_blue nb_button_forward", d);
        bf.innerText = "Check File";
        bf.addEventListener('click', create_server_d.onVerifyArkMapButtonPressed);

        ark.showNewCustomMenu(d, "new_custom_box_tall");
    },
    function(id) {
        var d = ark.createDom("div", "");
        ark.createDom("div","nb_title nb_big_padding_bottom", d).innerText = "Name Your New Server";
        ark.createDom("div","np_sub_title nb_big_padding_bottom", d).innerText = "Your new server is almost ready! Just give it a memorable name and it'll be all ready to go.";

        var entry = ark.createDom("input","nb_input_text", d);
        entry.id="nb_map_name";
        entry.value = "";

        ark.createDom("div", "error_text", d).id="nb_map_name_error";

        var bf = ark.createDom("div", "nb_button_blue nb_button_forward", d);
        bf.innerText = "Finish";
        bf.addEventListener('click', create_server_d.onFinishNamingMap);

        ark.showNewCustomMenu(d, "new_custom_box_tall");
    }
]

create_server_d.currentSession = null;
create_server_d.currentStep = 0;

create_server_d.getEventsTimer = null;
create_server_d.currentPort = "7779";
create_server_d.currentArkSavePath = "";

create_server_d.onCreate = function() {
    //Ask the server for an ID.
    ark.serverRequest("https://ark.romanport.com/api/users/@me/server_wizard/start", {}, function(d) {
        create_server_d.currentSession = d;
        create_server_d.currentStep = 0;
        create_server_d.currentPort = "7779";
        create_server_d.getEventsTimer = window.setInterval(create_server_d.onGetEventsTimerTick, 2000);
        create_server_d.showStep();
    });
}

create_server_d.sendMessage = function(typeId, data) {
    //Create message payload
    var payload = {
        "type":typeId,
        "data":data
    };

    //Semd
    ark.serverRequest(create_server_d.currentSession.request_url, {
        "type":"post",
        "body":JSON.stringify(payload)
    }, function(d) {
        if(!d.ok) {
            alert("Got NOT OK when sending message!");
        }
    });
}

create_server_d.onBackButtonPressed = function() {
    //Go back a step.
    create_server_d.currentStep--;

    //If this was the first page, stop
    if(create_server_d.currentStep == -1) {
        create_server_d.onEnd();
        return;
    }

    //Display next
    create_server_d.showStep();
}

create_server_d.onNextButtonPressed = function() {
    //Go forward a step.
    create_server_d.currentStep++;

    //Display next
    create_server_d.showStep();
}

create_server_d.onEnd = function() {
    //Stop timer
    clearInterval(create_server_d.getEventsTimer);

    //Hide window
    ark.hideCustomArea();
}

create_server_d.onGetEventsTimerTick = function() {
    //Get current events
    ark.serverRequest(create_server_d.currentSession.request_url, {}, function(d) {
        for(var i = 0; i<d.length; i+=1) {
            var de = d[i];
            console.log(de);
            create_server_d.incomingMessageActions[de.type](de.data, de.type, de.from_ip);
        }
    });
}

create_server_d.showStep = function() {
    create_server_d.steps[create_server_d.currentStep](create_server_d.currentSession.display_id);
}

create_server_d.onVerifyPortButtonPressed = function() {
    //Pull the port out
    create_server_d.currentPort = document.getElementById('port_box_input').value;

    //Switch views
    var d = ark.createDom("div", "");
    ark.createDom("div","nb_title nb_big_padding_bottom", d).innerText = "Validating Server";
    ark.createDom("div","np_sub_title nb_big_padding_bottom", d).innerText = "Waiting for the server to start the validation process...";
    ark.showNewCustomMenu(d, "new_custom_box_tall");

    //Submit
    create_server_d.sendMessage(2, {
        "port":create_server_d.currentPort
    });
}

create_server_d.onVerifyArkMapButtonPressed = function() {
    //Get
    var path = document.getElementById('nb_map_save_entry').value;
    create_server_d.currentArkSavePath = path;

    //Switch views
    var d = ark.createDom("div", "");
    ark.createDom("div","nb_title nb_big_padding_bottom", d).innerText = "Validating Map Location";
    ark.createDom("div","np_sub_title nb_big_padding_bottom", d).innerText = "Waiting for the server to start the validation process...";
    ark.showNewCustomMenu(d, "new_custom_box_tall");

    //Submit
    create_server_d.sendMessage(4, {
        "path":path
    });
}

create_server_d.onFinishNamingMap = function() {
    //Get name
    var name = document.getElementById('nb_map_name').value;

    //Validate name
    if(name.length > 24 || name.length < 2) {
        document.getElementById('nb_map_name_error').innerText = "Please keep names between 2-24 characters.";
        return;
    }

    //Submit map name
    ark.serverRequest("https://ark.romanport.com/api/servers/"+create_server_d.currentSession.server.id+"/rename", {
        "type":"post",
        "body":JSON.stringify({
            "name":name
        })
    }, function(d) {
        //Create config file
        var config = {
            "web_port":parseInt(create_server_d.currentPort),
            "auth":{
                "id":create_server_d.currentSession.server.id,
                "creds":create_server_d.currentSession.server.server_creds
            },
            "child_config":{
                "resources_url": "https://ark.romanport.com/resources",
                "api_url": "https://ark.romanport.com/api",
                "save_location":create_server_d.currentArkSavePath
            }
        };

        //Switch views
        var d = ark.createDom("div", "");
        ark.createDom("div","nb_title nb_big_padding_bottom", d).innerText = "Finishing Up";
        ark.createDom("div","np_sub_title nb_big_padding_bottom", d).innerText = "Submitting configuration file to server...";
        ark.showNewCustomMenu(d, "new_custom_box_tall");

        //Submit to server!
        create_server_d.sendMessage(6, {
            "config":JSON.stringify(config)
        });
    });

    
}

/* Actions */
create_server_d.onServerHello = function(data) {
    //If we're on the 1 step, continue.
    if(create_server_d.currentStep == 1) {
        create_server_d.onNextButtonPressed();
    }
}

create_server_d.onServerFinishStartingTestTcpServer = function(data, type, fromIp) {
    var failCallback = function() {
        //Falled on failure. Show error step
        create_server_d.currentStep+=1;
        create_server_d.showStep();
    };

    if(data["ok"] == "true") {
        //Test the server
        ark.serverRequest("https://ark.romanport.com/api/users/@me/server_wizard/test_tcp_connection?ip="+fromIp+"&port="+create_server_d.currentPort, {}, function(e) {
            if(e.ok) {
                //Pass! Move on to the next step.
                create_server_d.currentStep+=2;
                create_server_d.showStep();
            } else {
                //Fail
                failCallback();
            }
        });
    } else {
        //Fail now
        failCallback();
    }
    
}

create_server_d.onServerFinishMapCheck = function(data) {
    var exists = data["exists"] == "true";
    var valid = data["isValidArk"] == "true";

    //Decide where to go
    if(!valid) {
        //Skip forward a step so the back button works
        create_server_d.currentStep++;

        //Show depending on wether it is valid ark or file doesn't exist
        if(exists) {
            var d = ark.createDom("div", "");
            ark.createDom("div","nb_title nb_big_padding_bottom", d).innerText = "Testing Server Failed";
            ark.createDom("div","np_sub_title nb_big_padding_bottom", d).innerText = "Map file check failed! The file specified was not a valid ARK file. Make sure the file ends in .ark and begins with your map name.";
    
            var b = ark.createDom("div", "nb_button_blue nb_button_back", d);
            b.innerText = "Back";
            b.addEventListener('click', create_server_d.onBackButtonPressed);
    
            ark.showNewCustomMenu(d, "new_custom_box_tall");
        } else {
            var d = ark.createDom("div", "");
            ark.createDom("div","nb_title nb_big_padding_bottom", d).innerText = "Map File Not Found";
            ark.createDom("div","np_sub_title nb_big_padding_bottom", d).innerText = "Map file check failed! The file specified did not exist.";
    
            var b = ark.createDom("div", "nb_button_blue nb_button_back", d);
            b.innerText = "Back";
            b.addEventListener('click', create_server_d.onBackButtonPressed);
    
            ark.showNewCustomMenu(d, "new_custom_box_tall");
        }
        return;
    } else {
        //Valid! Go to final step.
        create_server_d.onNextButtonPressed();
    }
}

create_server_d.onGoodbye = function() {
    //Show ending
    var d = ark.createDom("div", "");
    ark.createDom("div","nb_title nb_big_padding_bottom", d).innerText = "You're All Set!";
    ark.createDom("div","np_sub_title nb_big_padding_bottom", d).innerText = "Your new server is ready! It'll appear in your server list once you join the ARK server. Thanks!";

    var bf = ark.createDom("div", "nb_button_blue nb_button_forward", d);
    bf.innerText = "Finish";
    bf.addEventListener('click', function() {
        create_server_d.onEnd();
        window.location.reload();
    });

    ark.showNewCustomMenu(d, "new_custom_box_tall");
}

create_server_d.incomingMessageActions = [
    null,
    create_server_d.onServerHello,
    null,
    create_server_d.onServerFinishStartingTestTcpServer,
    null,
    create_server_d.onServerFinishMapCheck,
    null,
    create_server_d.onGoodbye
];

