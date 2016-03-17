/**
 * Created by rebeccahan on 15-8-28.
 */


$(".back").click(function(){
    history.back()
});

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
var key = parse_parameters()["key"];

function conh(){
    var bodyh = $(window).height();
    var footerh = $("footer").height();
    var conh = $(window).height() - $("footer").height() - $("#titlearea").height()*2 - 10;
//        console.log(conh);
    return conh;
}
(function($){
    $.fn.autoTextarea = function(options) {
        var defaults={
            maxHeight:null,
            minHeight:$(this).height()
        };
        var opts = $.extend({},defaults,options);
        return $(this).each(function() {
            $(this).bind("paste cut keydown keyup focus blur",function(){
                var height,style=this.style;
                this.style.height =  opts.minHeight + 'px';
                if (this.scrollHeight > opts.minHeight) {
                    if (opts.maxHeight && this.scrollHeight > opts.maxHeight) {
                        height = opts.maxHeight;
                        style.overflowY = 'scroll';
                    } else {
                        height = this.scrollHeight;
                        style.overflowY = 'hidden';
                    }
                    style.height = height  + 'px';
                }
            });
        });
    };
})(jQuery);
function textin(){

    $("#content").autoTextarea({
        maxHeight:conh(),
        minHeight:$(this).height()
    })
}

$(document).ready(textin(),imgnum());

$("#addimg").click(function(){
    if($(".img").css("display") == "none"){
        $(".img").css("display","block");
//            textin();
    }else{
        $(".img").css("display","none");
//            textin();
    }
});

//    textarea获取焦点后图片预览消失
$("textarea").click(function(){
    $(".img").css("display","none");
});


function imgnum(){
    var imgnum = $(".img").children(".imgpiece").length;
    $(".imgnum").text(imgnum);
//    console.log(imgnum);
    if(imgnum == 0){
        $(".imgnum").css("display","none");
    }else{
        $(".imgnum").css("display","inline-block");
    }
    if (imgnum == 8){
        $("#add").css("display","none");
    }else{
        $("#add").css("display","inline-block");
    }

    return imgnum;
}


//     调用相册
$("#add").click( function getCamera(){
    javascript:myjavascript.getCamera();
});
function printInfo(uri){
    var newimg = '<span class="imgpiece"><img class="pic" src='+uri+'>'+'<img class="del" src="https://api.xiuxiu.babadeyan.com/media_root/img/forum/delete.png">'+' </span>'
    $("#add").before(newimg);
    imgnum();
    $(".del").click(function(){
        $(this).parent().remove();
        imgnum();
    });
}

<!--测试用-->
//    $("#add").click(function printInfo(uri){
//        var uri = "img/meow3.jpg";
//        var newimg = '<span class="imgpiece"><img class="pic" src='+uri+'>'+'<img class="del" src="img/delete.png">'+' </span>'
////        $(".img").children(".imgpiece").eq(0).before(newimg);
//        $("#add").before(newimg);
//        imgnum();
//    } );
<!---->
$("#submit").click(function(){
    var titletext = $("#titlearea").val()
    var contenttext = $("#content").val()
    if(titletext == "" && contenttext == ""){
        $(".mask").css("display","block");
        $("#unfilled").css("display","block");
        $(".mask").click(function(){
            $(this).css("display","none");
            $("#unsuccess").css("display","none");
            $("#unfilled").css("display","none");
        })
    }else{
        pppush();
    }

});

//    上传图片
function pppush(){
    $(".mask").css("display","block");
    $("#waiting").css("display","block");
    var titletext = $("#titlearea").val()
    var contenttext = $("#content").val()
    var imgarray = new Array();
    $("#imgcont").children(".imgpiece").each(function(i,n){
        var thisimg = $(n).children(".pic").attr("src");
        var srcs = new Array();
        srcs = thisimg.split("base64,");
        imgarray.push(srcs[1]);
//        console.log(imgarray[i]);
    });
    console.log(user_id);
    $.ajax({
        type:"POST",
        url:"https://api.xiuxiu.babadeyan.com/rest/api/forumposts/",
        data:{
            status: 1,
            post_image_1: imgarray[0],
            post_image_2: imgarray[1],
            post_image_3: imgarray[2],
            post_image_4: imgarray[3],
            post_image_5: imgarray[4],
            post_image_6: imgarray[5],
            post_image_7: imgarray[6],
            post_image_8: imgarray[7],
            title:titletext,
            content:contenttext,
            key:key,
            user_id: user_id,
            category:"https://api.xiuxiu.babadeyan.com/rest/api/forumcategories/"+category_id+"/",
            owner:"https://api.xiuxiu.babadeyan.com/rest/api/users/"+user_id+"/"

        },
        dataType:"json",
        success:function(){
            console.log("success")
            setTimeout("window.location.href='https://api.xiuxiu.babadeyan.com/forum/post/list/'+category_id+'/'+'?user_id='+user_id+'&category_id='+category_id+'&key='+key",3000);
        },
        error:function(){
//                $("#loading").css("display","none");
            $("#unsuccess").css("display","block")
            console.log("unsuccess")
            setTimeout('$(".mask").css("display","none")',8000);
            $(".mask").click(function(){
                $(this).css("display","none");
                $("#waiting").css("display","none");
                $("#unsuccess").css("display","none");
                $("#unfilled").css("display","none");
            })
        }
    })

}

