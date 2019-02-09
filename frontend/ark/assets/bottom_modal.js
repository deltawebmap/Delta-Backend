var bottom_modal = {};
bottom_modal.bottomModalQueue = [];
bottom_modal.bottomModalActive = false;

bottom_modal.showBottomModal = function(text, callback, className) {
    var i = {};
    i.type = 0;
    i.callback = callback;
    i.text = text;
    i.className = className;
	//If it's already active, push it the the queue.
	if(bottom_modal.bottomModalActive) {
		bottom_modal.bottomModalQueue.unshift(i);
	} else {
		//Display now.
		bottom_modal.forceShowBottomModal(i);
	}
};

bottom_modal.showPromptBottomModal = function(text, actions, className, force) {
    var i = {};
    i.type = 2;
    i.actions = actions;
    i.text = text;
    i.className = className;
	//If it's already active, push it the the queue.
	if(bottom_modal.bottomModalActive && !force) {
		bottom_modal.bottomModalQueue.unshift(i);
	} else {
		//Display now.
		bottom_modal.forceShowBottomModal(i);
	}
}

bottom_modal.showLoaderBottom = function(text, onBeginLoad, className, onDismissCallback) {
    var i = {};
    i.type = 1;
    i.callback = onDismissCallback;
    i.onBeginLoad = onBeginLoad;
    i.text = text;
    i.className = className;
	//If it's already active, push it the the queue.
	if(bottom_modal.bottomModalActive) {
		bottom_modal.bottomModalQueue.unshift(i);
	} else {
		//Display now.
		bottom_modal.forceShowBottomModal(i);
	}
}

bottom_modal.onNetError = function(text) {
    bottom_modal.showPromptBottomModal(text, [
        {
            "text":"Reload",
            "callback":function(){
                window.location.reload();
            }
        },
    ], "bottom_modal_error", true);
}

bottom_modal.reportError = function(text) {
	bottom_modal.showBottomModal(text, null, "bottom_modal_error");
}

bottom_modal.reportDone = function(text) {
    bottom_modal.showBottomModal(text, null, "bottom_modal_good");
}

bottom_modal.forceHideBottomModalNoArgs = function() {
    bottom_modal.forceHideBottomModal({
        "callback":null,
        "className":""
    });
}

bottom_modal.forceHideBottomModal = function(request) {
    var node = document.getElementById('bottom_modal_rpws');
    //Hide.
    node.className = "bottom_modal_rpws font "+request.className;
    window.setTimeout(function() {
        //Call callback
        if(request.callback != null) {
            request.callback();
        }
        //Completely hide the classname
        node.className = "bottom_modal_rpws font ";
        //Toggle flag
        bottom_modal.bottomModalActive = false;
        //If there is an item in the queue, show it.
        if(bottom_modal.bottomModalQueue.length >= 1) {
            var o = bottom_modal.bottomModalQueue.pop();
            bottom_modal.forceShowBottomModal(o);
        }
    }, 300);
}

bottom_modal.forceShowBottomModal = function(request) {
    var node = document.getElementById('bottom_modal_rpws');
    node.innerHTML = "";

    var text_node = document.createElement('div');
    text_node.innerText = request.text;
    text_node.className = "bottom_modal_text";
    node.appendChild(text_node);
    
	node.className = "bottom_modal_rpws bottom_modal_active_rpws font "+request.className;
    bottom_modal.bottomModalActive = true;
    
    //Called when it is time to dismiss this.
    var onDoneShowCallback = function() {
        bottom_modal.forceHideBottomModal(request);
    };

	if(request.type == 0) {
        //Standard wait. 
        window.setTimeout(onDoneShowCallback, 1600 + ((request.text.length / 12) * 1000));
    } else if(request.type == 1) {
        //Record current time. This'll be used when we come back.
        var startTime = new Date().getTime();
        //Load callback. Call the callback and expect a ping back shortly.
        request.onBeginLoad(function() {
            //Check if we've elapsed the time required to show the modal.
            var remainingTime = 300 - (new Date().getTime() - startTime);
            if(remainingTime > 0) {
                //Wait.
                window.setTimeout(onDoneShowCallback, remainingTime);
            } else {
                //Do it now. 
                onDoneShowCallback();
            }
        });
    } else if (request.type == 2) {
        //Actions
        for(var i = 0; i<request.actions.length; i+=1) {
            var a = request.actions[i];
            var n = document.createElement('div');
            n.innerText = a.text;
            n.className = "bottom_modal_btn";
            n.x_action = a.callback;
            n.addEventListener('click', function() {
                this.x_action();
                onDoneShowCallback();
            });
            node.appendChild(n);
        }
    }
}