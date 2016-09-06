
var TwitchAlerts = {
    // returns the followers JSON
    setTwitchAlert: function (channel,client_id) {
        var followerJson;
        var followersArray = [];
        var followsArrayLengthOld = Utils.getFollowersJson(channel, client_id)._total;
        console.log(followsArrayLengthOld);
        var followsArrayLengthNew;
        var followsArrayLengthDiff;
        var nameInterval;
        setInterval(function () {
            followerJson = Utils.getFollowersJson(channel);
            if(followerJson._total > 1){
                followsArrayLengthNew = followerJson._total;
                console.log(followsArrayLengthNew);
                if(followsArrayLengthOld < followsArrayLengthNew){
                    followsArrayLengthDiff = followsArrayLengthNew - followsArrayLengthOld;
                    for(var i=0; i<followsArrayLengthDiff; i++){
                        followersArray.push(followerJson.follows[i].user.display_name);
                    }
                    console.log(followersArray);
                    nameInterval = setInterval(function () {
                        if(followersArray.length > 0){
                            displayFollowerAlert(followersArray[0]);
                            followersArray.splice(0,1);
                        } else{
                            clearInterval(nameInterval);
                        }
                    },5000);
                    followsArrayLengthOld = followsArrayLengthNew;
                }
            }
        },10000);
    }
};
