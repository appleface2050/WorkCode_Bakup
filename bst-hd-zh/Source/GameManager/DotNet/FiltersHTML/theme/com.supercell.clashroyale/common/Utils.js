var Utils = {
    // returns the followers JSON
    getFollowersJson: function (channel, client_id) {
        var result="";
        var url = "https://api.twitch.tv/kraken/channels/" + channel + "/follows?dummy=" + Math.random();
        $.ajax({
            url:url,
            headers: {
                'Accept': 'application/vnd.twitchtv.v3+json',
                'Client-ID': client_id
            },
            async: false,
            success:function(data) {
                result = data;
            }
        });
        return result;
    }
};
