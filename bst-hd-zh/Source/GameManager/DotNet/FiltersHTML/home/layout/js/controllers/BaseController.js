app.controller('BaseController', function ($scope,$rootScope,$location,$window) {
    $scope.is_backend_ready = false;
    $scope.player = null;
    $scope.activeTheme = null;
    $scope.mode = null;

    $scope.init = function () {
        //Init function to fetch parameter from Url
        $scope.package = $location.search().appPkg;
    

        
        $scope.is_backend_ready = true;
        if ($location.search().activeMode != undefined) {
            //if URL contains some mode, set it in view.
            setModeSettings();
        } else if ($location.search().mode != undefined) {
            $scope.mode = $location.search().mode;
        }
        else {
            // setting mode to landscape by default
            $scope.mode = 'landscape';
        }

	if ($location.search().live == undefined) {
	    $scope.live = false;
	}
	else {
	    $scope.live = true;
	}
    };

    function setModeSettings(){
        console.log('in setmodesettings');
        console.log($location.search());
        //active mode will be in format of "landscape_1 / portrait_1"
        $scope.activeMode = $location.search().activeMode;
        console.log($scope.activeMode);
        $scope.mode = $location.search().activeMode.split('_')[0];
    }
});
