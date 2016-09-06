var GmApi = {

    callCSharpHandler: function(calledFunction, callbackFunction, dataObj) {
        var dataObj = {calledFunction: calledFunction, callbackFunction: callbackFunction, data: dataObj};
        var event = new MessageEvent('MessageEvent', { 'view': window, 'bubbles': false, 'cancelable': false, 'data': JSON.stringify(dataObj)});
        document.dispatchEvent(event);
    },

    gmReportProblem: function(){
        this.callCSharpHandler("ReportProblem", null, null);
    },

    gmGetGuid: function(guidCallback){
        this.callCSharpHandler("GetUserGUID", guidCallback, null);
    }
}
