app.factory('ApiFactory', function($q,$http,$rootScope,$location){
    return{
    	    Edit_filter_stats: function (event_type, reason,session_id,selected_filter,config_json) {
            // var rightNow = new Date();
            // var timezone = String(String(rightNow).split("(")[1]).split(")")[0];
            var deferred = $q.defer();
            var user_agent     = navigator.userAgent.split('/');
            var prod_ver_index = user_agent.length-1;
            var prod_ver       = user_agent[prod_ver_index];
            var guid = $location.search().guid;
            var sidebar_session_id = $location.search().session_id;
        

            $rootScope.stat_json = {
                    event_type: event_type,
                    cancelled_edit_filters_reason: reason,
                    guid:guid,
                    prod_ver:prod_ver,
                    filter_name:selected_filter,
                    filter_session_id:session_id,
                    session_id:sidebar_session_id,                    
                    filter_config_json:config_json
            }
            console.log($rootScope.stat_json);

            if (event_type != 'filter_applied' && event_type != 'cancelled_edit_filters') {
                    $http({
                             url: "https://bluestacks-cloud.appspot.com/stats/btvfunnelstats",
                                method: 'POST',
                                params: $rootScope.stat_json

                            }).success(function (data) {
                //                console.log("stop_stream response:", data);
                                deferred.resolve(data);
                            });
            }
            
            return deferred.promise;

        },

    }
});



