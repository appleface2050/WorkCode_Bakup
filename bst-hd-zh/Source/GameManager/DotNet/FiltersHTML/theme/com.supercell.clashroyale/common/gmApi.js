var GmApi = {

    callCSharpHandler: function(calledFunction, callbackFunction, dataObj) {
        var dataObj = {calledFunction: calledFunction, callbackFunction: callbackFunction, data: dataObj};
        var event = new MessageEvent('MessageEvent', { 'view': window, 'bubbles': false, 'cancelable': false, 'data': JSON.stringify(dataObj)});
        document.dispatchEvent(event);
    },

    gmEnableWebcam : function(width, height, position)
    {
        this.callCSharpHandler("EnableWebcam", null, [width, height, position]);
    },

    gmDisableWebcam : function()
    {
        this.callCSharpHandler("DisableWebcam", null, null);
    }
};
