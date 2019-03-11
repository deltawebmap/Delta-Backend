var ark_users = {};

ark_users.serverRequest = function(url, args, callback) {
    var xmlhttp = new XMLHttpRequest();
    xmlhttp.onreadystatechange = function () {
        if (this.readyState == 4 && this.status == 200) {
            callback(JSON.parse(this.responseText));
        } else if (this.readyState == 4 && this.status == 500) {
            //Known server error.
            var err = JSON.parse(this.responseText);
            if(err.error_code == 5) {
                //Jump to signin
                window.location = "/login/?callback="+encodeURIComponent(window.location);
                return;
            }
            ark_users.onNetError(err);
        } else if (this.readyState == 4) {
            //Parse the response and display the error
            if(args.customErrorCallback == null) {
                ark_users.onNetError(this.status + " ("+this.statusText+")", args);
            } else {
                args.customErrorCallback(this);
            }
            
        }
    }
    xmlhttp.ontimeout = function () {
        ark_users.onNetError("Timed out", args);
    }
    xmlhttp.onerror = function () {
        ark_users.onNetError("Generic error", args);
    }
    xmlhttp.onabort = function () {
        ark_users.onNetError("Request aborted", args);
    }
    if(args.method == null) {
        args.method = "GET";
    }
    xmlhttp.open(args.method, url, true);
    xmlhttp.withCredentials = true;
    if(args.nocreds != null) {
        xmlhttp.withCredentials = !args.nocreds;
    }
    xmlhttp.send(args.body);
}

ark_users.onNetError = function() {
    alert('net error');
}

ark_users.me = null;

ark_users.refreshUserData = function(callback) {
    ark_users.serverRequest("https://ark.romanport.com/api/users/@me/", {}, function(e) {
        ark_users.me = e;
        callback(e);
    });
}