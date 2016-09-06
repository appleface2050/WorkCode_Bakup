app.controller('BaseController', function ($scope,$rootScope,$location,$window) {
    $scope.is_backend_ready = false;
    $scope.player = null;
    $scope.activeTheme = null;

    // get the Json objects from the filterthemes.js file
    $scope.init = function () {
        $scope.package = $location.search().appPkg;
        $scope.themes = themes_json.themes[$scope.package];
        $scope.is_backend_ready = true;
        if($location.search().theme != undefined){
            //if URL contains some theme and settings, set it in view.
            setThemeSettings();
        }
         // GmApi.gmGetGuid("getGuid");
        console.log("I am Init");
        $window.getGuid = function(guid) {
        GmApi.gmLogInfo('got guid:' + guid);
        $scope.guid = guid;
        console.log("guid");
        console.log(guid);
    }; 
    };

    $rootScope.stringToBoolean = function(string){
        switch(string.toLowerCase().trim()){
            case "true": case "yes": case "1": return true;
            case "false": case "no": case "0": case null: return false;
            default: return Boolean(string);
        }
    }

    function setThemeSettings(){
        console.log($location.search());
        $scope.activeTheme = $scope.themes[$location.search().theme];
        $scope.config_json = $scope.themes[$location.search().theme].initial_config.settings;
        $rootScope.initial_config_json = $scope.config_json;
        console.log($scope.config_json);
        if($location.search().webcam !=undefined)
            $scope.config_json.webcam = $rootScope.stringToBoolean($location.search().webcam);
        if($location.search().chat !=undefined)
            $scope.config_json.chat = $rootScope.stringToBoolean($location.search().chat);
        if($location.search().animate !=undefined)
            $scope.config_json.animate = $rootScope.stringToBoolean($location.search().animate);
    }

});