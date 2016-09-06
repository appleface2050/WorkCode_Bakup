var GmApi = {

    callCSharpHandler: function(calledFunction, callbackFunction, dataObj) {
        var dataObj = {calledFunction: calledFunction, callbackFunction: callbackFunction, data: dataObj};
        var event = new MessageEvent('MessageEvent', { 'view': window, 'bubbles': false, 'cancelable': false, 'data': JSON.stringify(dataObj)});
        document.dispatchEvent(event);
    },
    gmCloseFilterWindow: function(stat_json){
        this.callCSharpHandler("CloseFilterWindow", null, [JSON.stringify(stat_json)]);
    },
    gmSelectTheme: function(theme){
        this.callCSharpHandler("ChangeFilterTheme", null, [theme]);
    },
    gmUpdateSettings: function(theme, config_json){
        this.callCSharpHandler("UpdateThemeSettings", null, [theme,config_json]);
    }
}
