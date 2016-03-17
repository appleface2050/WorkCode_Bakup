/**
 * Created by Rebecca_Han on 15/12/1.
 */



var timer = 3.0; //    定义计时器
//var haze = 1;
var sum = 0;  //    已答对题目数
var count;  //  总题目数

persec = $("#timebar").width() / timer;
//////////////////////////////////定义变量//////////////////////////////////

//    ///////////////////////////////js交互进程////////////////////////////////
$(document).ready(function(){
    //$(".container0").hide();
    //$(".container1").show();
    //$(".container3").show();
    //$(".container2").show();
    //$(".container4").show();
    //$(".container2").css("background-color","");

    $("#believe").click(function(){
        $(".container1").hide();
        $(".container1-2").show();
    });
    $("#unbelieve").click(function(){
        $(".container1").hide();
        $(".container1-1").show();
    });
    getque();

    $("#sendtel").click(function(){
        send();
    });
    $("#replay").click(function(){
        //location.reload();
        sum = 0;
        timer = 3.0;
        //$("#quenum").html(1);
        $("#timered").width(240);
        regetque();
    })

    var counttitle = "";
    var thisurl = window.location.href;
    var titlearray = new Array();
    titlearray = thisurl.split("?count-title=");
    counttitle = titlearray[1];
    counttitle = decodeURI(counttitle);

    if(counttitle!=""){
        $("download").click(downloadcount());
        function downloadcount(){
            $.ajax({
                type:"POST" ,
                url: "https://api.xiuxiu.babadeyan.com/rest/api/record/create" ,
                data:{title : counttitle },
                dataType:"json" ,
                success:function(){
                    console.log("downloadcount+1");
                },
                error: function(){
                    console.log("downloadcount failed");
                }
            });
        }
    }
});

//////////////////////////////////////函数定义///////////////////////////////
//    计时器
the_timer = null;
function Timer(t){
    console.log("t=" + t);
    $("#time").html(""+ t.toFixed(1));
    $("#timered").width(240);
    persec = $("#timebar").width() / (10*t);
    the_timer = setInterval(timerf, 100);

    function timerf(){
        $("#time").html(""+t);
        if (t <= 0){
            $(".container3").show();
            $("#sum").html(sum);
            $("#time").html("0");
            $("#timered").width(0);
            clearInterval(the_timer);
            the_timer = null;

            console.log("timeout");

        }else {
            if(the_timer == null){
                the_timer = setInterval(timerf, 100);
            }
            t = t - 0.1;
            $("#time").html(""+t.toFixed(1));
            var nextwidth = $("#timered").width() - persec;
            $("#timered").width(nextwidth);

        }
    }
}

function getque(){
    $.ajax({
        type:"GET" ,
        url: "https://api.xiuxiu.babadeyan.com/rest/api/games/graph" ,
//        data: ,
        dataType:"json" ,
        success:function(data){
            count = parseInt(data.count);
            quecouple = new Array();
            quecouple = data.data;
            $(".container0").hide();
            $(".container1").show();
            var q = 0; //  当前题目数
            $("#time").html(""+timer);
            $(".middle").click(function begin(){
                $(".container1-1").hide();
                $(".container1-2").hide();
                $(".container2").show();

                Timer(timer);
                queshow(q);
            });

        },
        error:function(){}
    });
}
function regetque(){
    $.ajax({
        type:"GET" ,
        url: "https://api.xiuxiu.babadeyan.com/rest/api/games/graph" ,
//        data: ,
        dataType:"json" ,
        success:function(data){
            count = parseInt(data.count);
            quecouple = new Array();
            quecouple = data.data;
            var q = 0; //  当前题目数
            $("#time").html(timer);
            $(".container4").hide();
            $(".container1-1").hide();
            $(".container1-2").hide();
            $(".container2").show();
            Timer(timer);
            queshow(q);

        },
        error:function(){}
    });
}



function queshow(q){
    $("#left").unbind("click");
    $("#right").unbind("click");
    console.log("NO."+q);
    var quebody = quecouple[q].questions;
    var leftone = quebody[0];
    var rightone = quebody[1];

    $("#left").css("background-color",leftone.color);
    $("#left").html(leftone.name);
    $("#right").css("background-color",rightone.color);
    $("#right").html(rightone.name);

    var answ = quecouple[q].answer;
    console.log("答案为"+answ);
    $("#left").click(function(){
        if(answ == 0){
            q++;
            sum = sum+1;

            clearInterval(the_timer);
            the_timer = null;

            Timer(timer);
            return queshow(q);

        }else if(answ == 1){
            clearInterval(the_timer);
            the_timer = null;
            $(".container3").show();
            $("#sum").html(sum);
            $("#time").html("0");
            $("#timered").width(0);
            //Timer(0);
        }
    });
    $("#right").click(function(){
        if(answ == 1){
            q++;
            sum = sum+1;

            clearInterval(the_timer);
            the_timer = null;

            Timer(timer);
            return queshow(q);

        }else if(answ == 0){
            clearInterval(the_timer);
            the_timer = null;
            $(".container3").show();
            $("#sum").html(sum);
            $("#time").html("0");
            $("#timered").width(0);
            //Timer(0);
        }
    });
}

function send(){
    $("title").html("雾霾吸多了,智商已透支,快来测测你的智商欠费了吗?");
    $.ajax({
        type:"POST" ,
        url: "https://api.xiuxiu.babadeyan.com/rest/api/games/set" ,
        data: {
            phone: $("#tel").val(),
            score: sum
        },
        dataType:"json" ,
        success:function(data){
            if (data.success){
                $("#ranknum").html(data.ranking);
            }else{

            }



        },
        error:function(){}
    });
    $(".container3").hide();
    $(".container4").show();
}


