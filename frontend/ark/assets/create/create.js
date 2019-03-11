//Init Flickity
var elem = document.querySelector('.main_carousel');
var flkty = new Flickity( elem, {
    // options
    cellAlign: 'center',
    contain: true,
    draggable: false,
    accessibility: true,
    prevNextButtons: false,
    pageDots: false
});

function SetStateBar(index) {
    //Find all state elements
    var circles = document.getElementsByClassName('percentage_item_circle');
    var bars = document.getElementsByClassName('percentage_item_rectangle');

    //Update circles
    for(var i = 0; i<circles.length; i+=1) {
        var e = circles[i];
        if(i > index) {
            e.classList.remove("sideburn_percentage_active");
        } else {
            e.classList.add("sideburn_percentage_active");
        }
    }

    //Update rectangles
    for(var i = 0; i<bars.length; i+=1) {
        var e = bars[i];
        if(i > index-1) {
            e.classList.remove("sideburn_percentage_active");
        } else {
            e.classList.add("sideburn_percentage_active");
        }
    }
}

function SetLoader(visble) {
    var e = document.getElementById('loader');
    if(visble) {
        e.style.display = "block";
    } else {
        e.style.display = "none";
    }
}

var steps = [
    {
        "pageIndex":1,
        "barIndex":0,
        "name":"Intro",
        "onopen":function(){},
    },
    {
        "pageIndex":2,
        "barIndex":1,
        "name":"Download",
        "onopen":function(){},
    },
    {
        "pageIndex":3,
        "barIndex":2,
        "name":"Connect",
        "onopen":function(){},
    },
    {
        "pageIndex":4,
        "barIndex":3,
        "name":"Networking Setup",
        "onopen":function(){},
    },
    {
        "pageIndex":5,
        "barIndex":3,
        "name":"Networking Status",
        "onopen":function(){},
    },
    {
        "pageIndex":6,
        "barIndex":4,
        "name":"Map Selection",
        "onopen":function(){
            //Check filepicker for later
            OnFilepickerType();
        },
    },
    {
        "pageIndex":7,
        "barIndex":5,
        "name":"Server Personalization",
        "onopen":function(){},
    },
    {
        "pageIndex":8,
        "barIndex":6,
        "name":"Sending Config",
        "onopen":SendFinalConfig,
    },
    {
        "pageIndex":9,
        "barIndex":6,
        "name":"Waiting",
        "onopen":AfterGoodbye,
    },
    {
        "pageIndex":10,
        "barIndex":7,
        "name":"Done",
        "onopen":function() {
            window.setTimeout(function() {
                window.location = "https://ark.romanport.com/";
            }, 5000);
        },
    }
]

var currentStep = -1;

function SendFinalConfig() {
    var c = {
        "web_port":serverPort,
        "child_config":{
            "resources_url":"https://ark.romanport.com/resources",
            "save_map":savedServerMap,
            "save_location":savedServerSaveLocation,
            "base_permissions":[
                "allowViewTamedTribeDinoStats",
                "allowSearchTamedTribeDinoInventories",
                "allowHeatmap",
                "allowHeatmapDinoFilter"
            ],
            "permissions_version":0
        },
        "auth":{
            "creds":server_data.server_creds,
            "id":server_data.id
        }
    }

    //Create payload to send
    var payload = {
        "config":JSON.stringify(c)
    }

    //Send message
    SendMessage(6, payload);
}

function AfterGoodbye() {
    //Wait for the server to start with a loop. Cancel the old, dead one.
    clearInterval(getEventsTimer);

    getEventsTimer = window.setInterval(PingServerTick, 1000);
}

function PingServerTick() {
    ark_users.serverRequest("https://ark.romanport.com/api/servers/"+server_data.id, {}, function(d) {
        if(d.online) {
            //Done. Show next step and finish.
            clearInterval(getEventsTimer);
            NextStep();
        }
    });
}

function SetStep(index) {
    //Grab step
    var s = steps[index];

    //Set the state bar
    SetStateBar(s.barIndex);

    //Jump to this page
    flkty.select(s.pageIndex);

    //Set current step
    currentStep = index;

    //Run code
    s.onopen();
}

function NextStep() {
    SetStep(currentStep + 1);
}

function BackStep() {
    SetStep(currentStep - 1);
}

function onGetEventsTimerTick() {
    //Ticked. Make a request to the server.
    ark_users.serverRequest(getEventsEndpoint, {}, function(evs) {
        for(var i = 0; i<evs.length; i+=1) {
            var e = evs[i];
            incomingMessageActions[e.type](e.data, e.type, e.from_ip);
        }
    });
}

function SendMessage(typeId, data) {
    //Create message payload
    var payload = {
        "type":typeId,
        "data":data
    };

    //Semd
    ark_users.serverRequest(getEventsEndpoint, {
        "method":"post",
        "body":JSON.stringify(payload)
    }, function(d) {
        if(!d.ok) {
            alert("Got NOT OK when sending message!");
        }
    });
}

var server_data;
var getEventsEndpoint;
var getEventsTimer;
var conf;

var serverPort;
var savedServerIconToken = null; //May be null if an icon is never uploaded.
var savedServerName = "Ark";
var savedServerMap;
var savedServerSaveLocation;

//Init now. Request user data to ensure we're logged in.
ark_users.refreshUserData(function(u) {
    //Request config
    ark_users.serverRequest("https://ark.romanport.com/client_config.json", {}, function(c) {
        conf = c;
        
        //Create session
        ark_users.serverRequest("https://ark.romanport.com/api/users/@me/server_wizard/start", {}, function(session) {
            server_data = session.server;
    
            //Start events loop.
            getEventsEndpoint = session.request_url;
            getEventsTimer = window.setInterval(onGetEventsTimerTick, 2000);
    
            //Set defaults
            serverPort = "7779";
            document.getElementById('port_box_input').value = serverPort;
    
            //Set code
            document.getElementById('auth_code').innerText = session.display_id;
    
            //Show step
            NextStep();
    
            //Hide loader
            SetLoader(false);
        });
    });
});

/* UI Events */
function OnVerifyPortBtnClicked() {
    //Update our port
    serverPort = document.getElementById('port_box_input').value;

    //Add loader
    SetLoader(true);

    //Send request
    SendMessage(2, {
        "port":serverPort
    });
}

var allowFilePickerNext = false;
function SetFilepickerContinueBtn(text, active) {
    var e = document.getElementById('filesearch_btn');
    e.innerText = text;
    if(active) {
        e.classList.remove("button_disabled");
    } else {
        e.classList.add("button_disabled");
    }
    allowFilePickerNext = active;
}

var latestFilepickerRequestId = 0;
function CheckFilepicker(query, mapName) {
    //Set button and get ID
    SetFilepickerContinueBtn("Checking...", false);
    var id = latestFilepickerRequestId++;

    //Set global
    savedServerSaveLocation = query;
    savedServerMap = mapName;

    //Create string
    var path = query;
    while(path.endsWith('/') || path.endsWith('\\')) {
        path = path.substring(0, path.length - 1);
    }
    path+="/"+mapName+".ark";

    //Create message
    var payload = {
        "path":path,
        "rid":id
    };

    //Send message
    SendMessage(4, payload);
}

function OnFilepickerType() {
    CheckFilepicker(document.getElementById('filesearch_path').value,document.getElementById('filesearch_map').value);
}

function OnFilepickerContinueBtn() {
    if(allowFilePickerNext) {
        NextStep();
    }
}

function OnImagePickerClick() {
    //Open file picker for image
    document.getElementById('image_picker').click();
}

function OnImagePickerChooseImage() {
    console.log("Chose server image. Uploading...");

    //Create form data
    var formData = new FormData();
    formData.append("f", document.getElementById('image_picker').files[0]);

    //Send
    ark_users.serverRequest("https://user-content.romanport.com/upload?application_id=Pc2Pk44XevX6C42m6Xu3Ag6J", {
        "method":"post",
        "body":formData,
        "nocreds":true
    }, function(f) {
        //Update the image here
        var e = document.getElementById('image_picker_image');
        if(e.firstChild != null) {
            e.firstChild.remove();
        }
        e.style.backgroundImage = "url('"+f.url+"')";
        savedServerIconToken = f.token;
    });
}

function OnEditServerName(context) {
    savedServerName = context.value;
    var er = document.getElementById('icon_picker_error');
    var e = document.getElementById('icon_picker_btn');
    if(savedServerName.length > 24 || savedServerName.length < 2) {
        er.style.display = "block";
        e.classList.add("button_disabled");
    } else {
        er.style.display = "none";
        e.classList.remove("button_disabled");
    }
}

function OnEditServerDone() {
    //Validate
    if(savedServerName.length <= 24 && savedServerName.length >= 2) {
        //Send request

        //Next step
        NextStep();
    }
}

/* Incoming events */
function onServerHello() {
    //Someone signed on with this ID.
    SetStep(3);
}

function onServerFinishStartingTestTcpServer(data, type, fromIp) {
    //Hide both elements
    var passedE = document.getElementById('nct_passed');
    var failedE = document.getElementById('nct_failed');
    passedE.style.display = "none";
    failedE.style.display = "none";

    //Set fail
    var failCallback = function() {
        //Show failure
        failedE.style.display = "inline-block";
        SetStep(4);

        //Turn off loader
        SetLoader(false);
    };

    if(data["ok"] == "true") {
        //Test the server
        ark_users.serverRequest("https://ark.romanport.com/api/users/@me/server_wizard/test_tcp_connection?ip="+fromIp+"&port="+encodeURIComponent(serverPort), {}, function(e) {
            if(e.ok) {
                //Pass! Move on to the next step.
                passedE.style.display = "inline-block";
                SetStep(4);

                //Turn off loader
                SetLoader(false);

                //In a moment, display the next step
                window.setTimeout(function() {
                    SetStep(5);
                }, 5000);
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

function onCheckFilepickerReply(data) {
    //Check to see if this is the latest reply
    if(parseInt(data["rid"]) != latestFilepickerRequestId - 1) {
        return;
    }

    //Set button
    var exists = data["exists"] == "true";
    var valid = data["isValidArk"] == "true";
    if(exists) {
        if(valid) {
            SetFilepickerContinueBtn("Continue", true);
        } else {
            SetFilepickerContinueBtn("Invalid .ARK", false);
        }
    } else {
        SetFilepickerContinueBtn("File Not Found", false);
    }
}

function onGoodbye() {
    NextStep();
}

var incomingMessageActions = [
    null,
    onServerHello,
    null,
    onServerFinishStartingTestTcpServer,
    null,
    onCheckFilepickerReply,
    null,
    onGoodbye
];
