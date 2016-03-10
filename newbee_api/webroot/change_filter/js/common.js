/*从客户端获取用户token和url*/

var bearer_token = getUrlParam("bearer_token");
var get_url = getUrlParam("get_url");
var cleaner_id = getUrlParam("cleaner_id");
var shareurl_d = [
				// "https://api.sangebaba.com/change_filter/send.html",
				"https://api.sangebaba.com/change_filter/send-2.html",
				"https://api.sangebaba.com/change_filter/send-3.html",
				"https://api.sangebaba.com/change_filter/send-4.html",
				"https://api.sangebaba.com/change_filter/send-5.html",
				"https://api.sangebaba.com/change_filter/send-6.html",
				"https://api.sangebaba.com/change_filter/send-7.html",
				"https://api.sangebaba.com/change_filter/send-8.html",
				"https://api.sangebaba.com/change_filter/send-9.html",
				"https://api.sangebaba.com/change_filter/send-10.html",
				"https://api.sangebaba.com/change_filter/send-11.html",
				];//分享链接
// var title = "我家的空气质量高于全国97%用户";
// var message = "为了家人健康购买了台“三个爸爸”空气净化器，始终保持室内空气清新。";
// var img_url ="https://api.sangebaba.com/change_filter/img/share.png";
var index_d = Math.floor((Math.random()*shareurl_d.length));
var shareurl = shareurl_d[index_d];
//var get_url = "http://api.release.sangebaba.com:80/v2";
//var bearer_token = "9be2bfb76d1a1ee5fa36f02bfb13259c";
//var cleaner_id = "60C5A8605299";


/*获取页面间传参函数*/

function getUrlParam(name)
{
    var reg = new RegExp("(^|&)"+ name +"=([^&]*)(&|$)");
    var r = window.location.search.substr(1).match(reg);
    if (r!=null) return unescape(r[2]); return null;
}