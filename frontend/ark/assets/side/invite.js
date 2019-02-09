var invite = {};

invite.reveal = function(inviterName, serverName, inviterIcon, serverIcon) {
    document.getElementById('a_inviter').innerText = inviterName;
    document.getElementById('a_server_name').innerText = serverName;

    //Show
    document.getElementById('box').classList.remove('frame_box_hidden');
}

invite.cancel = function() {
    window.location = "/";
}

invite.getInviteId = function() {
    var i = window.location.pathname.substring("/invite/".length);
    if(i == "") {
        invite.cancel();
    }
    return i;
}

invite.acceptUrl = null;

invite.onFail = function() {
    invite.cancel();
}

invite.init = function() {
    //Fetch user data to see if we're signed in.
    ark_users.refreshUserData(function(d) {
        //Fetch invite data
        ark_users.serverRequest("https://ark.romanport.com/api/invites/"+invite.getInviteId(), {"customErrorCallback":invite.onFail}, function(c) {
            console.log(c);
            invite.reveal(c.inviter_name, c.server.display_name, c.inviter_user_icon, c.server.image_url);
            invite.acceptUrl = "https://ark.romanport.com/api/users/@me/invites/accept/?id="+c.invite.id;
        });
    });
}

invite.onAcceptButtonPressed = function() {
    ark_users.refreshUserData(function(d) {
        //Fetch invite data
        ark_users.serverRequest(invite.acceptUrl, {}, function(c) {
            //Return
            invite.cancel();
        });
    });
}



invite.init();