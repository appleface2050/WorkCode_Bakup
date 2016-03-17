/**
 * Created by Rebecca_Han on 16/2/18.
 */
const url = window.location.href;

var articleid = url.match(/a=[1-9]{1}[0-9]*/g)+"";
var aid = articleid.split("=")[1];
var userid = url.match(/u=\d+/g)+"";
var uid = userid.split("=")[1];

$(document).ready(function(){
    $.ajax({
        type:"GET" ,
        url: "https://api.xiuxiu.babadeyan.com/rest/api/forumarticles/"+aid+"/" ,
        data:{},
        dataType:"json" ,
        success:
            function(data){
                $("title").html(data.title);
                $("#maintitle").html(data.title);
                $("#author").append(data.author);
                var date = data.created_at.split("T")[0];
                date = date.split("-");
                date = date[0]+"/"+date[1]+"/"+date[2];
                $("#date").html(date);
                $("#content").append(data.content);

            },
        error:function(){
            $(".container").html("啊哦~ 文章好像丢失了呢~");
        }
    });
    $.ajax({
        type:"POST" ,
        url: "https://api.xiuxiu.babadeyan.com/rest/api/forum/browse" ,
        data:{
            user_id: uid,
            article_id: aid
        },
        dataType:"json" ,
        success:
            function(data){
                if(data.status == 1){
                    console.log("browse+1")
                }else{
                    console.log("browse failed")
                }
            },
        error:function(){
            console.log("browse failed")
        }
    });
});

var behaviorid = url.match(/b=[A-Z]{1,3}_(A|B)\d{5}S\d{1}/g)+"";
bid = behaviorid.split("=")[1];
var stepid = parseInt(bid.match(/\d{1,2}$/g));
var behaveid = bid.replace(/\d{1,2}$/g, "");
var nbid = behaveid + (stepid+1);
var nnbid = behaveid + (stepid+2);


var externalid = url.match(/ex=(0|1)/g)+"";
var exid = externalid.split("=")[1];
console.log(exid);

if(exid == 1){
    $(".recommend").css("display", "block");
    $("#download").css("display", "block");
    $(".container").css("margin", "0 auto 55px")
    $(document).ready(function(){
        $.ajax({
            type:"POST" ,
            url: "https://api.xiuxiu.babadeyan.com/rest/api/sharestatistics/" ,
            data:{
                user_id : uid,
                behavior_name : nbid
            },
            dataType:"json" ,
            success:function(){
                console.log("open+1");
            },
            error: function(){
                console.log("opencount failed");
            }
        });
        $("#download").click(function(){
            $.ajax({
                type:"POST" ,
                url: "https://api.xiuxiu.babadeyan.com/rest/api/sharestatistics/" ,
                data:{
                    user_id : uid,
                    behavior_name : nnbid
                },
                dataType:"json" ,
                success:function(){
                    console.log("download+1");
                },
                error: function(){
                    console.log("downloadcout failed");
                }
            });
        });
    });

}





