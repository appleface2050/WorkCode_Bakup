var GmApi = {
    //生成全局数据时的防止重复增量
    _gid: 0,
    apkIsInstalledHandleMap: {},
    callCSharpHandler: function (calledFunction, callbackFunction, dataObjArg) {

        //debug
        //if (callbackFunction) return window[callbackFunction](true);

        var dataObj = {
            calledFunction: calledFunction,
            callbackFunction: callbackFunction,
            data: dataObjArg
        };
        var event = new MessageEvent('MessageEvent', {
            'view': window,
            'bubbles': false,
            'cancelable': false,
            'data': JSON.stringify(dataObj)
        });
        document.dispatchEvent(event);
    },
    checkIsInstalledFn: function (gmApi, pkg) {
        var callbackFunName = "__gmApiCB" + (++gmApi._gid);
        window[callbackFunName] = function (isInstalled) {
            isInstalled = isInstalled == "True";
            //本回调是一次性的,因此进来后直接销毁
            delete window[callbackFunName];
            var nv = gmApi.apkIsInstalledHandleMap[pkg];
            if (isInstalled) {
                //终于安装了!
                nv.callback();
                //开始销毁
                nv.callback = null;
                delete gmApi.apkIsInstalledHandleMap[pkg];
                return;
            }
            //继续等待检测
            nv.timeHandle = setTimeout(gmApi.checkIsInstalledFn, 1000, gmApi, pkg);
            //(由于模拟器内核不支持setTimeout,因此改成耗性能的回调完接着查)
            //gmApi.checkIsInstalledFn(gmApi, pkg);
            gmApi = null;
        };
        gmApi.callCSharpHandler("isAppInstalled", callbackFunName, [pkg]);
    },
    startCheckIsInstalled: function (pkg, callback) {
        /// <summary>
        /// 开始检测包的安装结果,会无限等待下去,如果同个包重复检测,会中断之前的并使用新的开始(触发使用新的回调)
        /// </summary>
        /// <param name="pkg" type="String"></param>
        /// <param name="callback" type="Function"></param>

        var nv = this.apkIsInstalledHandleMap[pkg];
        if (nv) {
            clearTimeout(nv.timeHandle);
        } else {
            nv = this.apkIsInstalledHandleMap[pkg] = {};
        }
        nv.callback = callback;
        //首次检测马上触发
        this.checkIsInstalledFn(this, pkg);
    },

    gmShowApp: function (displayName, packageName, activityName, apkUrl, installedCallback) {
        /// <summary>
        /// 启动或者开始下载应用
        /// </summary>
        /// <param name="displayName" type="String">启动界面的应用显示名</param>
        /// <param name="packageName" type="String">包名</param>
        /// <param name="activityName" type="String">可能是选项卡标识,传""</param>
        /// <param name="apkUrl" type="String">包的下载URL</param>
        /// <param name="installedCallback" type="Function">新下载安装完成则回调,已安装过的也会触发回调</param>
        if (activityName == "") {
            activityName = ".Main";
        }
        this.callCSharpHandler("ShowApp", null, [displayName, packageName, activityName, apkUrl]);

        if (installedCallback) {
            //需要安装完成回调
            this.startCheckIsInstalled(packageName, installedCallback);
        }
    },

    _checkPkgIsInstalledQueueRunning: false,
    _checkPkgIsInstalledQueue: [],
    _gmIsAppInstalledNext: function () {
        /// <summary>
        /// 根据包名检测是否安装,下一个队列元素
        /// </summary>

        this._checkPkgIsInstalledQueueRunning = true;
        var nextItem = this._checkPkgIsInstalledQueue.shift();
        if (!nextItem) {
            this._checkPkgIsInstalledQueueRunning = false;
            return;//没了
        }

        var pkg = nextItem.pkg;
        var callback = nextItem.callback;
        var t = this;

        var callbackFunName = "__gmApiCB" + (++this._gid);
        window[callbackFunName] = function (isInstalled) {
            //本回调是一次性的,因此进来后直接销毁
            delete window[callbackFunName];
            callback.apply(this, [isInstalled == "True"]);
            callback = null;
            nextItem = null;
            //下一个
            t._gmIsAppInstalledNext();
            t = null;
        };
        this.callCSharpHandler("isAppInstalled", callbackFunName, [pkg]);
    },
    gmIsAppInstalled: function (pkg, callback) {
        /// <summary>
        /// 根据包名检测是否安装
        /// </summary>
        /// <param name="pkg" type="String"></param>
        /// <param name="callback" type="Function"></param>

        //加入检测队列
        this._checkPkgIsInstalledQueue.push({ pkg: pkg, callback: callback });

        if (!this._checkPkgIsInstalledQueueRunning) {
            //首次启动
            this._gmIsAppInstalledNext();
        }
    },






    gmStartAppDownload: function (pkg, apkUrl) {
        this.callCSharpHandler("startCDNAppDownload", null, [pkg, apkUrl]);
    },

    gmGetAppDownloadProgress: function (pkg, callback) {
        /// <summary>
        /// 获取应用下载进度
        /// </summary>
        /// <param name="pkg" type="String"></param>
        /// <param name="callback" type="Function"></param>
        var callbackFunName = "__gmApiCB" + (++this._gid);
        window[callbackFunName] = function () {
            //本回调是持续调用,因此需要根据逻辑去判断销毁时间
            //delete window[callbackFunName];
            callback.apply(this, arguments);
            //callback = null;
        };
        this.callCSharpHandler("getAppDownloadProgress", callbackFunName, [pkg]);
    },




    gmRelaunchApp: function (displayName, packageName, activityName, apkUrl) {
        //alert(displayName+","+packageName+","+activityName+","+apkUrl);
        if (activityName == "") {
            activityName = ".Main";
        }
        this.callCSharpHandler("relaunchApp", null, [displayName, packageName, activityName, apkUrl]);
    },

    gmGoHome: function () {
        this.callCSharpHandler("GetClientId", null, []);
    },

    gmShowWebPage: function (title, webUrl) {
        this.callCSharpHandler("showWebPage", null, [title, webUrl]);
    },

    gmGetThemesJson: function () {
        alert("to be implemented with Gecko: gmGetThemesJson");
        try {
            return JSON.parse(window.external.GetThemesJson());
        }
        catch (e) {
            //console.log(e);
            return JSON.parse("[]");
        }
    },

    gmStageCompleted: function (stage) {
        this.gmStageCompleted("StageCompleted", null, null);
    },

    gmInTabBar: function (callBackFunction) {
        this.callCSharpHandler("inTabBar", callBackFunction, null);
    },

    gmCloseCurrentTab: function () {
        this.callCSharpHandler("CloseCurrentTab", null, null);
    },

    gmReloadFailedUrl: function () {
        this.callCSharpHandler("reloadFailedUrl", null, null);
    },

    gmShowMyAppsLocal: function () {
        this.callCSharpHandler("showMyAppsLocal", null, null);
    },

    gmIsBlueStacksInstalled: function () {
        return true;//window.external.IsBlueStacksInstalled();
    },

    gmUninstallApp: function (pkg, callBackFunction) {
        this.callCSharpHandler("uninstallApp", callBackFunction, [pkg]);
    },

    gmCreateAppShortcut: function (pkg) {
        this.callCSharpHandler("createAppShortcut", null, [pkg]);
    },

    gmReportProblem: function () {
        this.callCSharpHandler("ReportProblem", null, null);
    },

    gmRestartAndroidPlugin: function () {
        this.callCSharpHandler("RestartAndroidPlugin", null, null);
    },

    gmCheckForUpdates: function () {
        this.callCSharpHandler("CheckForUpdates", null, null);
    },

    gmMyAccount: function () {
        this.callCSharpHandler("showMyAccount", null, null);
    },

    gmLaunchHelp: function () {
        this.callCSharpHandler("launchHelpApp", null, null);
    },

    gmLaunchLanguageInput: function () {
        this.callCSharpHandler("launchLanguageInputApp", null, null);
    },

    gmShowFoneLink: function () {
        this.callCSharpHandler("ShowFoneLink", null, null);
    },

    getUserGUID: function (callBackFunction) {
       //alert("to be implemented by Gecko: getUserGUID");
       // return window.external.GetUserGUID();
        this.callCSharpHandler("GetUserGUID", callBackFunction, null);
    },

    //theme specific
    gmLaunchHome: function () {
        this.callCSharpHandler('LaunchHome', null, null);
    },

    //theme specific
    gmLaunchSearch: function (search_string) {
        this.callCSharpHandler("LaunchSearch", null, [search_string]);
    },

    //theme specific
    gmLaunchThemesPage: function () {
        this.callCSharpHandler("LaunchThemesPage", null, null);
    },

    gmMakeWebCall: function (url, script_tobe_invoked) {
        //script_tobe_invoked(htmlResult); will be called when done
        try {
            this.callCSharpHandler("makeWebCall", null, [url, script_tobe_invoked]);
        }
        catch (e) {
            //alert("exception in C# communication with PlayStore");
        }
    },

    gmInstallOnDevice: function (pkgName) {
        alert("to be implemented by Gecko: gmInstallOnDevice");
        return window.external.InstallOnDevice(pkgName);
    },

    gmGetAvailableLocaleName: function () {
        alert("to be implemented by Gecko: gmGetAvailableLocaleName");
        return window.external.GetAvailableLocaleName();
    },

    gmSetTheme: function (name, theme_base_url, download_url) {
        this.callCSharpHandler("SetTheme", null, [name, theme_base_url, download_url]);
    },

    gmGetCurrentTheme: function () {
        alert("to be implemented by Gecko: gmGetCurrentTheme");
        return window.external.GetCurrentTheme();
    },

    gmStartStream: function (key, loc, callbackFunction) {
        this.callCSharpHandler("StartStream", null, [key, loc, callbackFunction]);
    },

    gmStopStream: function () {
        this.callCSharpHandler("StopStream", null, null);
    },

    gmSetSystemVolume: function (level) {
        this.callCSharpHandler("SetSystemVolume", null, [level]);
    },

    gmSetMicVolume: function (level) {
        this.callCSharpHandler("SetMicVolume", null, [level]);
    },
}
