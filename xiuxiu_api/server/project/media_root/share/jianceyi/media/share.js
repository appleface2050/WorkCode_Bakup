/**
 * Created by rebeccahan on 15-6-30.
 */


$(document).ready(
    function data(){
        var uid = 0;
        var behaviorid = window.location.href.match(/b=CF_(URL|QRC)SB_W(XS|XT|B)_\d{1}/g)+"";
        var behaviorids = new Array();
        behaviorids = behaviorid.split("=");
        bid = behaviorids[1];
        var bid_id = parseInt(bid.match(/\d+/g));
        var bid_b = bid.match(/CF_(URL|QRC)SB_W(XS|XT|B)_/g)+"";

        var userid = window.location.href.match(/u=\d+/g)+"";
        var userids = new Array();
        userids = userid.split("=");
        uid = userids[1];

        var pid = window.location.href.match(/id=\d+/g)+"";
        var pids = new Array();
        pids = pid.split("=");
        id = pids[1];
        console.log(bid+"**"+uid+"**"+id);

        $.ajax({
            type:"POST" ,
            url: "https://api.xiuxiu.babadeyan.com/rest/api/sharestatistics/" ,
            data:{
                user_id : uid,
                behavior_name : bid_b + (bid_id+1)
            },
            dataType:"json" ,
            success:function(){
                console.log("open+1");
            },
            error: function(){
                console.log("opencount failed");
            }
        });
        $(".comment, .good, .bad, #downloada").click(function(){
            document.location.href = "https://api.xiuxiu.babadeyan.com/media_root/share/downloadpage/share2download.html?id="+id+"&b="+bid_b+(bid_id+1)+"&u="+uid;
        });

        $.ajax({
            type:"GET" ,
            url: "https://api.xiuxiu.babadeyan.com/rest/api/main/discovery?start_id="+id+"&single=1" ,
            data:{},
            dataType:"json" ,
            success:
                function(data){
                    var key = data.data[0]
//                    console.log(key);
//                    console.log(key.big_category_key);
                    $("title").text("来嗅嗅找好空气，免费领口罩");
                    if(key.bonus > 0){
                        $(".redbag").css("display","block");
                    }
                    $(".shoppic").attr("src",key.shop_image)
                    $(".shopname").text(key.shop_name);

                    if(key.shop_rate){
                        $(".star").attr("src",key.shop_rate);
                    }else{
                        $(".star").css("display","none");
                        $(".star1").css("display","inline-block");
                    }

                    if(key.price == null && key.shop_rate == ""){
                        $(".grade").css("display","none");
                        $(".price").css("display","none");
                    }else if(key.shop_price == null){
                        $(".price").css("display","none");
                    }else{
                        $(".price").text(key.shop_price+"元/人");
                    }

                    $(".dis").text(key.city);
                    $(".type").text(key.big_category_name);
                    $(".usericoni").attr("src",key.user_big_image);
                    $(".username").text(key.user_nickname);
                    if (key.month == 0){
                        if(key.days == 0){
                            $(".pubtime").text("今天");
                        }else{
                            $(".pubtime").text(key.days + "天前");
                        }
                    } else{
                        $(".pubtime").text(key.months + "月前");
                    }

                    if(key.PM2_5 == "0"||key.PM2_5 == "-1"||key.PM2_5 == null){
                        $(".pm25").css("display","none");
                        $(".datafrom").css("display","none");
                        $("#formaldehyde").css("display","inline-block");
                        $("#f-num").text(key.formaldehyde);
                        $("#f-degree").text(key.FORMALDEHYDE_level);
                        $(".pubpic").css("background","url("+key.formaldehyde_image+") no-repeat");
                        $(".pubpic").css("background-size", "100% auto");
                    }else{
                        $("#pm2_5").text(key.PM2_5);
                        //console.log(key.PM2_5);
                        $("#degree").text(key.PM2_5_degree);
                        $(".pubpic").css("background","url("+key.publish_image_url+") no-repeat");
                        $(".pubpic").css("background-size", "100% auto");
                    }

                    $(".money").text(key.bonus);
//                        console.log(key.bonus);
                    tags(key);


                    var datetime = key.created_at
                    var timearray1 = new Array();
                    timearray1 = datetime.split("T");
                    var date = timearray1[0]
                    var timearray2 = new Array();
                    timearray2 = timearray1[1].split("Z");
                    var timearray3 = new Array();
                    timearray3 = timearray2[0].split(":");
                    var time = timearray3[0]+":"+timearray3[1]
                    //console.log(time);
                    var date_time = date+" "+time
                    $(".datetime").text(date_time);

                    $(".description").text(key.content)
                    var comment = key.comment
                    var comnum = comment.count
//                    console.log(comnum)
                    $("#connum").text(comnum);

                    $("#goodnum").text(key.win);
                    $("#badnum").text(key.lost);
                },
            error:function(){
            }
        })
    }
);

function tags(key){
    var keytag = key.category_operation
    output = ""
    for (var key in keytag) {
        output += "<span class='tagtxt'>" + keytag[key] + "</span>";
    }
    $(".tags").append(output);
}
$(document).ready(
    activity()
);
function activity(){
    drop= parseInt(Math.random()*10);
    //console.log(drop%2);
    counttitle = "热门显示免费红包";
    $("#dlredbag").show();
    //if(drop%2 == 1){
    //    counttitle = "热门显示免费红包";
    //    $("#dlredbag").show();
    //    $("#downloada").attr("href","http://app.xiu.sangebaba.com/?count-title=下载from数据分享-红包免费送");
    //}else{
    //    counttitle = "热门显示游戏";
    //    $("#game").show();
    //    $("#downloada").attr("href","http://app.xiu.sangebaba.com/?count-title=下载from数据分享-显示游戏");
    //}
}



