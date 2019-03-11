var perms = {};

//Permissions list
perms.permissionsList = [
    "allowViewTamedTribeDinoStats",
    "allowSearchTamedTribeDinoInventories",
    "allowHeatmap",
    "allowHeatmapDinoFilter"
];

//Elements to enable when a permission is checked. These will be disabled when hide all is called.
perms.valuesToEnable = {
};

//Run the key when enabled
perms.functionsToRunOnEnable = {

};

//Run the key when disabled.
perms.functionsToRunOnDisable = {

};

perms.p = [];

perms.check = function(name) {
    return perms.p.includes(name);
};

perms.togglePerm = function(name, allow) {
    if(allow) {
        //Enabled
        if(perms.valuesToEnable[name] != null) {
            var a = perms.valuesToEnable[name];
            for(var i = 0; i<a.length; i+=1) {
                document.getElementById(a[i]).style.display = null;
            }
        }
        if(perms.functionsToRunOnEnable[name] != null) {
            perms.functionsToRunOnEnable[name]();
        }
    } else {
        //Disabled
        if(perms.valuesToEnable[name] != null) {
            var a = perms.valuesToEnable[name];
            for(var i = 0; i<a.length; i+=1) {
                document.getElementById(a[i]).style.display = "none";
            }
        }
        if(perms.functionsToRunOnDisable[name] != null) {
            perms.functionsToRunOnDisable[name]();
        }
    }
}

perms.refreshPerms = function() {
    for(var i = 0; i<perms.permissionsList.length; i+=1) {
        var s = perms.permissionsList[i];
        perms.togglePerm(s, perms.check(s));
    }
}