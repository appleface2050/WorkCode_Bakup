app.factory('ApiFactory', function($q,$http,$rootScope,$location){
    return{
    	    Edit_filter_stats: function (event_type, reason,session_id,selected_layout,status) {
            // var rightNow = new Date();
            // var timezone = String(String(rightNow).split("(")[1]).split(")")[0];
            var deferred = $q.defer();
            var user_agent     = navigator.userAgent.split('/');
            var prod_ver_index = user_agent.length-1;
            var prod_ver       = user_agent[prod_ver_index];
            var guid = $location.search().guid;
            var userLang = navigator.language || navigator.userLanguage;
            var facebook_name = $location.search().facebook_name;
            var fb_id = $location.search().facebook_id;
            var session_id_btv=$location.search().session_btv;
            if(status){
                var state= "Live";
            }
            else{
                var state = "offline";
            }

            //var sidebar_session_id = $location.search().session_id;
        

            $rootScope.stat_json = {
                    event_type: event_type,
                    layout_session_id:session_id,
                    cancelled_layout_reason: reason,
                    guid:guid,
                    user_lang:userLang,
                    prod_ver:prod_ver,
                    facebook_name:facebook_name,
                    streamer_name_online:fb_id,
                    layout_name:selected_layout,
                    state:state,
                    session_id:session_id_btv
                
            }
            console.log($rootScope.stat_json);
                    $http({
                             url: "https://bluestacks-cloud.appspot.com/stats/btvfunnelstats",
                                method: 'POST',
                                params: $rootScope.stat_json
                            }).success(function (data) {
                                deferred.resolve(data);
                            });
            
            return deferred.promise;

        },

    }
});



