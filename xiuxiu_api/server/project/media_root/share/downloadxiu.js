/**
 * Created by Rebecca_Han on 16/2/29.
 */


var browser = {
    versions: function() {
        var u = navigator.userAgent, app = navigator.appVersion;
        return {//移动终端浏览器版本信息
            trident: u.indexOf('Trident') > -1, //IE内核
            presto: u.indexOf('Presto') > -1, //opera内核
            webKit: u.indexOf('AppleWebKit') > -1, //苹果、谷歌内核
            gecko: u.indexOf('Gecko') > -1 && u.indexOf('KHTML') == -1, //火狐内核
            mobile: !!u.match(/AppleWebKit.*Mobile.*/) || !!u.match(/AppleWebKit/), //是否为移动终端
            ios: !!u.match(/\(i[^;]+;( U;)? CPU.+Mac OS X/), //ios终端
            android: u.indexOf('Android') > -1 || u.indexOf('Linux') > -1, //android终端或者uc浏览器
            iPhone: u.indexOf('iPhone') > -1 || u.indexOf('Mac') > -1, //是否为iPhone或者QQHD浏览器
            iPad: u.indexOf('iPad') > -1, //是否iPad
            webApp: u.indexOf('Safari') == -1 //是否web应该程序，没有头部与底部
        };
    }(),
    language: (navigator.browserLanguage || navigator.language).toLowerCase()
};
var iosd = "https://itunes.apple.com/cn/app/xiu-xiu-shi-nei-kong-qi-zhi/id1034278491?mt=8";
var androidd = "http://xiu.sangebaba.com/app/Xiu_pz_release_1.2.0.apk";

var ua = navigator.userAgent.toLowerCase();
if(ua.match(/MicroMessenger/i)=="micromessenger") {
    androidd = "http://a.app.qq.com/o/simple.jsp?pkgname=com.sangebaba.airdetetor";
    document.getElementById("download").href = androidd;
} else {
    if (browser.versions.ios || browser.versions.iPhone || browser.versions.iPad) {
        document.getElementById("download").href = iosd;
    }
    else if (browser.versions.android) {
        document.getElementById("download").href = androidd;
    }else{
        document.getElementById("unavailable").style.display = "block";
    }
}