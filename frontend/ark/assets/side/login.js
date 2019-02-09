function StartLoading() {
    document.getElementById('box').classList.add("frame_box_loading");
}

function StopLoading() {
    document.getElementById('box').classList.remove("frame_box_loading");
}

function GetFormData() {
    return {
        "username":document.getElementById('input_username').value,
        "password":document.getElementById('input_password').value,
    }
}

function parseURLParams() { 
	try {	
		var query = window.location.search;
		var objects = String(query).trim("?").split("&");
		//Create keys and objects.
		var i =0;
		var keys = [];
		var obj = {};
		while(i<objects.length) {
			try {
				var o = objects[i];
				//Trim beginning
				o = o.replace("?","").replace("&","");
				//Split by equals.
				var oo = o.split("=");
				keys.push(oo[0]);
				//Uri decode both of these
				var key = decodeURIComponent(oo[0]);
				var value = decodeURIComponent(oo[1]);
				obj[key] = value;
			} catch (e) {

			}
			i+=1;
		}
		return obj;
	} catch (ex) {
		return {};
	}
}

function OnGotReply(d) {
    if(d.ok) {
        //Go back
        var urlParams = parseURLParams();
        if(urlParams["callback"] == null) {
            window.location = "/";
        } else {
            window.location = urlParams["callback"];
        }
    } else {
        //Show error
        StopLoading();
        document.getElementById('error_txt').innerText = d.message;
    }
}

function OnSignupBtnPressed() {
    StartLoading();
    ark_users.serverRequest("https://ark.romanport.com/api/auth/password/create", {"method":"post", "body":JSON.stringify(GetFormData())}, function(d) {
        OnGotReply(d);
    });
}

function OnSigninBtnPressed() {
    StartLoading();
    ark_users.serverRequest("https://ark.romanport.com/api/auth/password/login", {"method":"post", "body":JSON.stringify(GetFormData())}, function(d) {
        OnGotReply(d);
    });
}