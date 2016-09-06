app.controller('FilterController', function ($scope, $location, $window,ApiFactory,$q,$timeout,$rootScope) {

    $scope.personalize_section = false;
    $scope.state = 'select_filter';
    $scope.session_id = Number(new Date());
    // $scope.config_json = ""


    //to change state to 'select_filter' and  'personalize'
    $scope.change_state = function (state) {
        $scope.state = state;
    };
    
   
    // change banner properties on theme selection
    $scope.select_theme = function (theme) {
        console.log('selecting theme:' + theme.dir_name);
        GmApi.gmSelectTheme(theme.dir_name);
        $scope.activeTheme = theme;
        ApiFactory
            .Edit_filter_stats('filter_selected',null,$scope.session_id,theme.display_name)
            .then(function (data) {
                console.log('data:');
                console.log(data);
            });
    };  
    $scope.checked = function (option) {
        console.log('in checked');
        $scope.config_json[option] = !$scope.config_json[option];
        console.log("$scope.config_json");
        console.log($scope.config_json);
        GmApi.gmUpdateSettings($scope.activeTheme.dir_name, JSON.stringify($scope.config_json));
    };

    $window.setSettings = function (config_string) {
      console.log('in $window.setSettings');
        $scope.config_json = JSON.parse(config_string);
        $scope.$apply();
    };

    $scope.closeFilterWindow = function () {
        console.log('closing filter window');
        ApiFactory
            .Edit_filter_stats('cancelled_edit_filters','Clicked_on_x_button',$scope.session_id,$scope.activeTheme.display_name,$scope.config_json);
        GmApi.gmCloseFilterWindow([$rootScope.stat_json]);
    };

    $scope.filter_done_Clicked =function(){
        // ApiFactory
            // .Edit_filter_stats('personalize_complete',null,$scope.session_id,$scope.activeTheme.display_name,$scope.config_json)
            ApiFactory
            .Edit_filter_stats('filter_applied',null,$scope.session_id,$scope.activeTheme.display_name,$scope.config_json);
            GmApi.gmCloseFilterWindow([$rootScope.stat_json]);

    }

    

    $scope.ShowPersonalize_section = function () {
        console.log($scope.activeTheme.display_name);
        $scope.state = 'personalize';
        $scope.personalize_section = $scope.personalize_section ? false : true;
         ApiFactory
            .Edit_filter_stats('personalize_clicked',null,$scope.session_id,$scope.activeTheme.display_name)
        
    };
});
