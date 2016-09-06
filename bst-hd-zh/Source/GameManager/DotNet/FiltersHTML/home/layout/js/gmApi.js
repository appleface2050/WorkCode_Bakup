var GmApi = {

    callCSharpHandler: function(calledFunction, callbackFunction, dataObj) {
        var dataObj = {calledFunction: calledFunction, callbackFunction: callbackFunction, data: dataObj};
        var event = new MessageEvent('MessageEvent', { 'view': window, 'bubbles': false, 'cancelable': false, 'data': JSON.stringify(dataObj)});
        document.dispatchEvent(event);
    },
    gmCloseDialog: function(stat_json){
        this.callCSharpHandler("CloseDialog", null, [JSON.stringify(stat_json)]);
    },
    gmDialogClickHandler: function(modeJson){
        this.callCSharpHandler("DialogClickHandler", null, [modeJson]);
    }
};
