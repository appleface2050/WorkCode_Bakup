/**
 * Created by rebeccahan on 15-8-28.
 */


//pulldown and pullup
//pulldown and pullup
//pulldown and pullup
//pulldown and pullup
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
var post_id = parse_parameters().post_id;
var key = parse_parameters()["key"];

var startid = parseInt($("#start_id").attr("value"));
var originid = startid;

var myScroll,
    pullDownEl, pullDownOffset,
    pullUpEl, pullUpOffset,
    generatedCount = 0;

function pullDownAction () {
//    $(".head").css("margin-top","-10px");
//    $("#pullDown").css("margin-top","40px");
    setTimeout("refreshdata()", 2000);	// <-- Simulate network congestion, remove setTimeout from production!
}

function refreshdata() {
    $.ajax({
        type:"GET" ,
        url: "https://api.xiuxiu.babadeyan.com/forum/reply/list/data/" ,
        data:{
            post_id: post_id,
            start_id: originid
        },
        dataType:"json" ,
        success:function(data){
            $(".comment-piece").remove();
            var tie = data.data;
            $("#start_id").attr("value",data.start_id);
            for(var i = 0 , len = tie.length; i<len; i++){
                var tt = tie[i];
                var li = '<div class="comment-piece"><div class="usericon">'+'<img src='+tt.small_owner_image+'></div><div class="username">'
                    +'<span class="name">'+tt.owner_nickname+'</span><span class="floor"><span class="flnum">'+tt.layer+'</span>楼</span>'
                    +'<span class="pubtime">'+tt.show_time+'</span><a class="re"><span>回复<img src="https://api.xiuxiu.babadeyan.com/media_root/img/forum/repeat.png"></span></a>'
                    +'<input class="parent_id" type="hidden" value='+tt.id+'/></div>'+'<div class="comment-detail"><p>'+tt.content+'</p>'
                if(tt.image1!==null){
                    li = li + '<img src='+tt.image1+'>';
                }
                if(tt.image2!==null){
                    li = li + '<img src='+tt.image2+'>';
                }
                if(tt.image3!==null){
                    li = li + '<img src='+tt.image3+'>';
                }
                if(tt.image4!==null){
                    li = li + '<img src='+tt.image4+'>';
                }
                if(tt.image5!==null){
                    li = li + '<img src='+tt.image5+'>';
                }
                if(tt.image6!==null){
                    li = li + '<img src='+tt.image6+'>';
                }
                if(tt.image7!==null){
                    li = li + '<img src='+tt.image7+'>';
                }
                if(tt.image8!==null){
                    li = li + '<img src='+tt.image8+'>';
                }
                li = li + '</div>';

                if (tt.parent_reply == 0){
                    li += '</div>'
                }else{
                    li += '<div class="quote"><span class="quote-user">'+tt.parent_reply_owner_nickname+'</span>：<span class="quote-con">'
                        +tt.parent_reply_content+'</span></div></div>'
                }
                var el = $('.comment');
                el.append(li);
                pullDownEl.querySelector('.pullDownLabel').innerHTML = '下拉刷新';

                action();
            }
        },
        error:function(){
            pullDownEl.querySelector('.pullDownLabel').innerHTML = '加载失败，点击重新加载';
            $(".pullDownLabel").click(function(){
                pullDownAction ();
            });
        }
    });
    myScroll.refresh();		// Remember to refresh when contents are loaded (ie: on ajax completion)
}
function pullUpAction () {
    setTimeout(function () {
        pullUpEl.querySelector('.pullUpLabel').innerHTML = '加载中......';
        //var startid = parseInt($("#start_id").attr("value"));
        //originid = startid;
        if(startid == -1){
            $("#nomore").css("display","block");
            $("#pullUp").remove();
        }else{
            $.ajax({
                type:"GET" ,
                url: "https://api.xiuxiu.babadeyan.com/forum/reply/list/data/" ,
                data:{
                    post_id:post_id,
                    start_id:startid
                },
                dataType:"json" ,
                success:function(data){
                    $("#start_id").attr("value",data.start_id);
                    var tie = data.data;
                    for(var i = 0 , len = tie.length; i<len; i++){
                        var tt = tie[i];
                        var li = '<li class="comment-piece"><div class="usericon">'+'<img src='+tt.small_owner_image+'></div><div class="username">'
                            +'<span class="name">'+tt.owner_nickname+'</span><span class="floor"><span class="flnum">'+tt.layer+'</span>楼</span>'
                            +'<span class="pubtime">'+tt.show_time+'</span><a class="re"><span>回复<img src="https://api.xiuxiu.babadeyan.com/media_root/img/forum/repeat.png"></span></a>'
                            +'<input class="parent_id" type="hidden" value='+tt.id+'/></div>'+'<div class="comment-detail"><p>'+tt.content+'</p>'
                            +'<img src='+tt.image1+'/>'+'<img src='+tt.image2+'/>'+'<img src='+tt.image3+'/>'+'<img src='+tt.image4+'/>'
                            +'<img src='+tt.image5+'/>'+'<img src='+tt.image6+'/>'+'<img src='+tt.image7+'/>'+'<img src='+tt.image8+'/>'
                            +'</div>'
                        if (tt.parent_reply == 0){
                            li += '</li>'
                        }else{
                            li += '<div class="quote"><span class="quote-user">'+tt.parent_reply_owner_nickname+'</span>：<span class="quote-con">'
                                +tt.parent_reply_content+'</span></div></li>'
                        }
                        var el = $('.comment');
                        el.append(li);
                        pullUpEl.querySelector('.pullUpLabel').innerHTML = '上拉加载更多';

                        myScroll.refresh();
                        action();
                    }
                },
                error:function(){
                    pullUpEl.querySelector('.pullUpLabel').innerHTML = '加载失败，点击重新加载';
                    $(".pullUpLabel").click(function(){
                        pullUpAction ();
                    });
                }
            })
        }

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
        topOffset: pullDownOffset,
        hideScrollbar: true,
        bindToWrapper: true,
        checkDOMChanges: false,
        useTransform:false,
        hScrollbar:false,
        vScrollbar:false,
//        momentum: false,
        onBeforeScrollStart: function (e) {
            var target = e.target;
            while (target.nodeType != 1) target = target.parentNode;

            if (target.tagName != 'P')
                e.preventDefault();
        },
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



//post all the texts and images
//post all the texts and images
//post all the texts and images
action();
function action(){
    $(".back").click(function(){
        history.back()
    });

    $("#share").click(function(){
        var href = "https://api.xiuxiu.babadeyan.com/forum/post/share/"+post_id;
        var title = document.title;
        javascript:myjavascript.shareWeChat(href,title);
    });
    $(".imgcont").click(function(){
        $("#big").css("display","block");
        $("#bigimg").attr("src",$(this).attr("src"));
    })
    $("#big").click(function(){
        $(this).css("display","none");
    })

//    $(".back").attr("href","https://api.xiuxiu.babadeyan.com/forum/post/list/"+category_id+"/"+"?user_id="+user_id+"&category_id="+category_id+"&key="+key);
    $("#zan").click(function(){
        var althis = $(this);
        if(user_id == 0){
            $(".mask").css("display","block");
            $("#unlogin").css("display","block");
            $(".mask").click(function(){
                $(this).css("display","none");
                $("#unsuccess").css("display","none");
                $("#unfilled").css("display","none");
                $("#unlogin").css("display","none");
                $("#unzan").css("display","none");
            })
        }else{
            if(althis.attr("class") == "good"){
                $.ajax({
                    type:"POST",
                    url:"https://api.xiuxiu.babadeyan.com/forum/post/detail/add/win/",
                    data:{
                        key:key,
                        user_id:user_id ,
                        forum_post_id:post_id
                    },
                    dataType:"json" ,
                    success:function(data){
                        var con = data.success;
                        good(con);

                    },
                    error:function(){
                        $(".mask").css("display","block");
                        $("#unzan").css("display","block");
                        $(".mask").click(function(){
                            $(this).css("display","none");
                            $("#unsuccess").css("display","none");
                            $("#unfilled").css("display","none");
                            $("#unlogin").css("display","none");
                            $("#unzan").css("display","none");
                        })
                    }
                });
                function good(con){
                    if(con == true){
                        althis.removeClass("good").addClass("gooded");
                        althis.children("img").attr("src","/media_root/img/forum/zan1.png");
                    }else{
                        $(".mask").css("display","block");
                        $("#unzan").css("display","block");
                        $(".mask").click(function(){
                            $(this).css("display","none");
                            $("#unsuccess").css("display","none");
                            $("#unfilled").css("display","none");
                            $("#unlogin").css("display","none");
                            $("#unzan").css("display","none");
                        })
                        console.log("点赞不成功");
                    }
                }
            }else{
                $.ajax({
                    type:"POST",
                    url:"https://api.xiuxiu.babadeyan.com/forum/post/detail/cancel/win/",
                    data:{
                        key:key,
                        user_id:user_id ,
                        forum_post_id:post_id
                    },
                    dataType:"json" ,
                    success:function(data){
                        var con = data.success;
                        gooded(con);
                    },
                    error:function(){
                    $(".mask").css("display","block");
                    $("#unzan").css("display","block");
                    $(".mask").click(function(){
                        $(this).css("display","none");
                        $("#unsuccess").css("display","none");
                        $("#unfilled").css("display","none");
                        $("#unlogin").css("display","none");
                        $("#unzan").css("display","none");
                    })
                }
                });
                function gooded(con){
                    if(con == true){
                        althis.removeClass("gooded").addClass("good");
                        althis.children("img").attr("src","/media_root/img/forum/zan.png");
//
                    }else{
                        console.log("取消赞不成功");
                        $(".mask").css("display","block");
                        $("#unzan").css("display","block");
                        $(".mask").click(function(){
                            $(this).css("display","none");
                            $("#unsuccess").css("display","none");
                            $("#unfilled").css("display","none");
                            $("#unlogin").css("display","none");
                            $("#unzan").css("display","none");
                        })
                    }
                }
            }
        }

    });

    $(".re").click(function(){
        if(user_id == 0){
            $(".mask").css("display","block");
            $("#unlogin").css("display","block");
            $(".mask").click(function(){
                $(this).css("display","none");
                $("#unsuccess").css("display","none");
                $("#unfilled").css("display","none");
                $("#unlogin").css("display","none");
                $("#unzan").css("display","none");
            })
        }else{
            $("#textin").focus();
            var username = $(this).parents(".username").children(".name").text();
            $("#textin").attr("placeholder","回复 "+username+"：");
            var parent = $(this).next().attr("value");
            $("#textin").attr("title",parent);
            console.log(username);
        }

    });
}


function relouzhu(){
    setTimeout('$("#textin").attr("placeholder","回复 楼主：")',1000);
    //$("#textin").attr("placeholder","回复 楼主：");

}

$(".add").click(function(){
    if($(".img").css("display") == "none"){
        $(".img").css("display","block");
//            textin();
    }else{
        $(".img").css("display","none");
//            textin();
    }
});
function imgnum(){
    var imgnum = $(".img").children(".imgpiece").length;
    $(".imgnum").text(imgnum);
    console.log(imgnum);
    if (imgnum == 8){
        $("#add").css("display","none");
    }else{
        $("#add").css("display","inline-block");
    }

    return imgnum;
}
$(".del").click(function(){
    $(this).parent().remove();
    imgnum();
});
$("#add").click( function getCamera(){
    javascript:myjavascript.getCamera();
});
//$("#add").click( function(){
//    if(user_id == 0){
//        javascript:myjavascript.login();
//    }else{
//        function getCamera(){
//            javascript:myjavascript.getCamera();
//        }
//        getCamera();
//    }
//});
//
function printInfo(uri){
    var newimg = '<span class="imgpiece"><img class="pic" src='+uri+'>'+'<img class="del" src="https://api.xiuxiu.babadeyan.com/media_root/img/forum/delete.png">'+' </span>'
    $("#add").before(newimg);
    imgnum();
}

<!--测试用-->
//            $("#add").click(function printInfo(uri){
//                var uri = "img/meow3.jpg";
//                var newimg = '<span class="imgpiece"><img class="pic" src='+uri+'>'+'<img class="del" src="img/delete.png">'+' </span>'
//                $(".img").children(".imgpiece").eq(0).before(newimg);
//                imgnum();
//            } );
<!---->

$("#submit").click(function(){
    $("#textin").focus();
    var contenttext = $("#textin").val();
    if(user_id == 0){
        $(".mask").css("display","block");
        $("#unlogin").css("display","block");
        $(".mask").click(function(){
            $(this).css("display","none");
            $("#unsuccess").css("display","none");
            $("#unfilled").css("display","none");
            $("#unlogin").css("display","none");
            $("#unzan").css("display","none");
        })
    }else{
        if(contenttext == ""){
            $(".mask").css("display","block");
            $("#unfilled").css("display","block");
            $(".mask").click(function(){
                $(this).css("display","none");
                $("#unsuccess").css("display","none");
                $("#unfilled").css("display","none");
                $("#unlogin").css("display","none");
                $("#unzan").css("display","none");
            })
        }else{
            pppush();
        }

    }
});

//    上传

function pppush(){
    $(".mask").css("display","block");
    $("#waiting").css("display","block");
    var contenttext = $("#textin").val()
    var imgarray = new Array();
    $("#imgcont").children(".imgpiece").each(function(i,n){
        var thisimg = $(n).children(".pic").attr("src");
        var srcs = new Array();
        srcs = thisimg.split("base64,");
        imgarray.push(srcs[1]);
    });
    var parent_id = parseInt($("#textin").attr("title"));
    if($("#textin").attr("placeholder") == "回复 楼主："){
        parent_id = 0;
    }
    console.log(parent_id);
    $.ajax({
        type:"POST",
        url:"https://api.xiuxiu.babadeyan.com/rest/api/forumreplies/",
        data:{
            status: 1,
            reply_image_1: imgarray[0],
            reply_image_2: imgarray[1],
            reply_image_3: imgarray[2],
            reply_image_4: imgarray[3],
            reply_image_5: imgarray[4],
            reply_image_6: imgarray[5],
            reply_image_7: imgarray[6],
            reply_image_8: imgarray[7],
            content:contenttext,
            parent_reply:parent_id,
            key:key,
            user_id: user_id,
            post:"https://api.xiuxiu.babadeyan.com/rest/api/forumposts/"+post_id+"/",
            owner:"https://api.xiuxiu.babadeyan.com/rest/api/users/"+user_id+"/"

        },
        dataType:"json",
        success:function(){
            console.log("success");
            setTimeout(function(){
                refreshdata();
                $(".mask").css("display","none");
                $("#waiting").css("display","none");
                $("#textin").val("");
                $("#textin").attr("title","0");
            }, 3000);
        },
        error:function(){
//                    $("#loading").css("display","none");
            $("#unsuccess").css("display","block")
            console.log("unsuccess")
            setTimeout('$(".mask").css("display","none")',10000);
            $(".mask").click(function(){
                $(this).css("display","none");
                $("#waiting").css("display","none");
                $("#unsuccess").css("display","none");
                $("#unfilled").css("display","none");
                $("#unlogin").css("display","none");
                $("#unzan").css("display","none");
            })
        }
    })

}
