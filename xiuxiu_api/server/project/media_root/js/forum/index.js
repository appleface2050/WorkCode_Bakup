/**
 * Created by rebeccahan on 15-8-28.
 */

function parse_parameters()
{
    var data = {};
    var loc = window.location.search;
    params = loc.substr(1);

    params_array = params.split("&");
    for(var i=0; i<params_array.length; i++)
    {
        var temp = params_array[i];
        var temp_array = temp.split("=");
        if (temp_array.length > 1)
        {
            data[temp_array[0]] = temp_array[1]
        }
    }

    return data
}
parse_parameters();
var user_id = parse_parameters().user_id;
var category_id = parse_parameters().category_id;
category_id = window.location.href.match(/list\/[0-9]+\/+/g).toString().split("/")[1];
var key = parse_parameters()["key"];





var myScroll,
    pullDownEl, pullDownOffset,
    pullUpEl, pullUpOffset,
    generatedCount = 0;

function pullDownAction () {
    setTimeout(function () {	// <-- Simulate network congestion, remove setTimeout from production!
        //$(".mask").css("display","block");
        //$("#refresh").css("display","block");
        //location.reload();

        pullDownEl.querySelector('.pullDownLabel').innerHTML = '加载中......';
        $("li").remove();
        $.ajax({
            type:"GET" ,
            url: "https://api.xiuxiu.babadeyan.com/forum/post/list/data/" ,
            data:{
                category_id:category_id,
                start_id:0
            },
            dataType:"json" ,
            success:function(data){
                var tie = data.data;
                if(tie.length == 0){
                    $("#nomore").css("display","block");
                    $("#pullUp").remove();
                }else{
                    for(var i = 0; i<3; i++){
                        var tt = tie[i];
                        var li = '<li><a href='+tt.next_url+' id=post_'+tt.id+'></a></li>';
                        var el = $('#thelist');
                        el.append(li);
                        var next = '<div class="content"><div class="content-head"><div class="user"><img src='+tt.big_user_image+'><span class="username">'+tt.user_nickname+'</span>'+'<span class="pubtime">'+tt.show_time+'</span>'
                            +'<span class="zan"><img src="https://api.xiuxiu.babadeyan.com/media_root/img/forum/good.png">'+tt.win_count+'</span>'+'<span class="renum"><img src="https://api.xiuxiu.babadeyan.com/media_root/img/forum/re.png">'+tt.reply_count+'</span>'
                            +'</div></div><div class="content-detail"> <div class="content-title">'
                            +'<p>'+tt.title+'</p>'+'</div><div class="content-abstrat">'
                            +'<p>'+tt.content+'</p>'
                            +'</div><div class="content-img">';
                        if(tt.image1!==null){
                            next = next + '<img src='+tt.image1+'>';
                        }
                        if(tt.image2!==null){
                            next = next + '<img src='+tt.image2+'>';
                        }
                        if(tt.image3!==null){
                            next = next + '<img src='+tt.image3+'>';
                        }
                        next = next + '</div></div>'+'</div>';
                        var al = $("li:last");
                        var aal = al.find("a:first");
                        aal.append(next);
                        var imgwidth = $(".content-img img:first").width();
                        $(".content-img").children("img").css("height",imgwidth+"px");
                        pullDownEl.querySelector('.pullDownLabel').innerHTML = '下拉刷新';
                        myScroll.refresh();
                    }
                }
            },
            error:function(){
                pullDownEl.querySelector('.pullDownLabel').innerHTML = '加载失败，点击重新加载';
                $(".pullDownLabel").click(function(){
                    pullDownAction ();
                });
            }
        })


        $(".mask").hide();
        myScroll.refresh();		// Remember to refresh when contents are loaded (ie: on ajax completion)
    }, 1000);	// <-- Simulate network congestion, remove setTimeout from production!
}

function pullUpAction () {
    setTimeout(function () {	// <-- Simulate network congestion, remove setTimeout from production!
        pullUpEl.querySelector('.pullUpLabel').innerHTML = '加载中......';
        var lastid = $("li:last").find("a:first").attr("id");
        var idarry = new Array();
        idarry = lastid.split("_");
        var startid = parseInt(idarry[1]);

        //console.log(startid);
        //if(startid == -1){
        //    $("#nomore").css("display","block");
        //    $("#pullUp").remove();
        //}else{
        $.ajax({
            type:"GET" ,
            url: "https://api.xiuxiu.babadeyan.com/forum/post/list/data/" ,
            data:{
                category_id:category_id,
                start_id:startid
            },
            dataType:"json" ,
            success:function(data){
                var tie = data.data;
                if(tie.length == 0){
                    $("#nomore").css("display","block");
                    $("#pullUp").remove();
                }else{
                    for(var i = 0 , len = tie.length; i<len; i++){
                        var tt = tie[i];
                        var li = '<li><a href='+tt.next_url+' id=post_'+tt.id+'></a></li>';

                        var el = $('#thelist');
                        el.append(li);
                        var next = '<div class="content"><div class="content-head"><div class="user"><img src='+tt.big_user_image+'><span class="username">'+tt.user_nickname+'</span>'+'<span class="pubtime">'+tt.show_time+'</span>'
                            +'<span class="zan"><img src="https://api.xiuxiu.babadeyan.com/media_root/img/forum/good.png">'+tt.win_count+'</span>'+'<span class="renum"><img src="https://api.xiuxiu.babadeyan.com/media_root/img/forum/re.png">'+tt.reply_count+'</span>'
                            +'</div></div><div class="content-detail"> <div class="content-title">'
                            +'<p>'+tt.title+'</p>'+'</div><div class="content-abstrat">'
                            +'<p>'+tt.content+'</p>'
                            +'</div><div class="content-img">';
                        if(tt.image1!==null){
                            next = next + '<img src='+tt.image1+'>';
                        }
                        if(tt.image2!==null){
                            next = next + '<img src='+tt.image2+'>';
                        }
                        if(tt.image3!==null){
                            next = next + '<img src='+tt.image3+'>';
                        }
                        next = next + '</div></div>'+'</div>';
                        var al = $("li:last");
                        var aal = al.find("a:first");
                        aal.append(next);
                        var imgwidth = $(".content-img img:first").width();
                        $(".content-img").children("img").css("height",imgwidth+"px");
                        pullUpEl.querySelector('.pullUpLabel').innerHTML = '上拉加载更多';
                        myScroll.refresh();
                    }
                }
            },
            error:function(){
                pullUpEl.querySelector('.pullUpLabel').innerHTML = '加载失败，点击重新加载';
                $(".pullUpLabel").click(function(){
                    pullUpAction ();
                });
            }
        })


        myScroll.refresh();		// Remember to refresh when contents are loaded (ie: on ajax completion)
    }, 1000);	// <-- Simulate network congestion, remove setTimeout from production!
}

function loaded() {
    pullDownEl = document.getElementById('pullDown');
    pullDownOffset = pullDownEl.offsetHeight;
    pullUpEl = document.getElementById('pullUp');
    pullUpOffset = pullUpEl.offsetHeight;

    myScroll = new iScroll('wrapper', {
        useTransition: false,
        useTransform: false,
        topOffset: pullDownOffset,
        hideScrollbar: true,
        bindToWrapper: true,
        checkDOMChanges: false,
        hScrollbar:false,
        vScrollbar:false,
//        momentum: false,
        onRefresh: function () {
            if (pullDownEl.className.match('loading')) {
                pullDownEl.className = '';
                pullDownEl.querySelector('.pullDownLabel').innerHTML = '下拉刷新';
            } else if (pullUpEl.className.match('loading')) {
                pullUpEl.className = '';
                        pullUpEl.querySelector('.pullUpLabel').innerHTML = '上拉加载更多';
            }
        },
        onScrollMove: function () {
            if (this.y > 5 && !pullDownEl.className.match('flip')) {
                pullDownEl.className = 'flip';
                pullDownEl.querySelector('.pullDownLabel').innerHTML = '下拉刷新';
                this.minScrollY = 0;
            } else if (this.y < 5 && pullDownEl.className.match('flip')) {
                pullDownEl.className = '';
                pullDownEl.querySelector('.pullDownLabel').innerHTML =  '下拉刷新';
                this.minScrollY = -pullDownOffset;
            } else if (this.y < (this.maxScrollY - 5) && !pullUpEl.className.match('flip')) {
                pullUpEl.className = 'flip';
                pullUpEl.querySelector('.pullUpLabel').innerHTML = '上拉加载更多';
//                        this.maxScrollY = this.maxScrollY;
            } else if (this.y > (this.maxScrollY + 5) && pullUpEl.className.match('flip')) {
                pullUpEl.className = '';
                pullUpEl.querySelector('.pullUpLabel').innerHTML = '上拉加载更多';
                this.maxScrollY = pullUpOffset;
            }
        },
        onScrollEnd: function () {
            if (pullDownEl.className.match('flip')) {
                pullDownEl.className = 'loading';
                pullDownEl.querySelector('.pullDownLabel').innerHTML = '加载中......';
                pullDownEl.querySelector('.pullDownIconImg').src = '/media_root/img/forum/loading2.png';
                pullDownAction();	// Execute custom function (ajax call?)
            } else if (pullUpEl.className.match('flip')) {
                pullUpEl.className = 'loading';
                pullUpEl.querySelector('.pullUpLabel').innerHTML = '加载中......';
                pullUpAction();	// Execute custom function (ajax call?)
            }
        }
    });

    $("#c-title").click(function(){
        myScroll.scrollTo(0,-60);
    });
    setTimeout(function () { document.getElementById('wrapper').style.left = '0'; }, 800);
}

document.addEventListener('touchmove', function (e) { e.preventDefault(); }, false);

document.addEventListener('DOMContentLoaded', function () { setTimeout(loaded, 200); }, false);


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

if (browser.versions.ios || browser.versions.iPhone || browser.versions.iPad) {
    $(".back").attr("href","https://api.xiuxiu.babadeyan.com/forum/index/?user_id="+user_id+"&key="+key);

}
else if (browser.versions.android) {
    $(".back").click(function(){
        javascript:myjavascript.goBack();
    });
}


$("#new").click(function(){

    if(typeof(user_id) == "undefined" || user_id == 0){
    //if(user_id == 0){
    //if(typeof(user_id)=="undefined" || user_id=="undefined"||user_id==0){
        $(".mask").css("display","block");
        $("#unlogin").css("display","block");
        $(".mask").click(function(){
            $(this).css("display","none");
        })
    }else{
        window.location.href = "https://api.xiuxiu.babadeyan.com/forum/post/create/?user_id="+user_id+"&category_id="+category_id+"&key="+key;
    }
});

$(".concern").click(function(){
    var althis = $(this);
    console.log(category_id);
    if(user_id == 0){
        $(".mask").css("display","block");
        $("#unlogin").css("display","block");
        $(".mask").click(function(){
            $(this).css("display","none");
            $("#unlogin").css("display","none");
        })
    }else{
        $.ajax({
            type:"POST",
            url:"https://api.xiuxiu.babadeyan.com/forum/post/list/add/concern/",
            data:{
                key:key,
                user_id: user_id,
                forum_category_id:category_id
            },
            dataType:"json" ,
            success:function(data){
                var con = data.success;
                console.log(con);
                con_change(con);
            },
            error:function(){
                $(".mask").css("display","block");
                $("#unsuccess").css("display","block");
                $(".mask").click(function(){
                    $(this).css("display","none");
                    $("#unsuccess").css("display","none");
                })
            }
        })
        function con_change(con){
            if(con == true){
                var concerned =  '<a class="concerned"><span><img src="https://api.xiuxiu.babadeyan.com/media_root/img/forum/attentioned.png"></span></a>'
                althis.after(concerned);
                althis.remove();
            }else{
                console.log("关注不成功");
                $(".mask").css("display","block");
                $("#unsuccess").css("display","block");
                $(".mask").click(function(){
                    $(this).css("display","none");
                    $("#unsuccess").css("display","none");
                })
            }
        }
    }
})
