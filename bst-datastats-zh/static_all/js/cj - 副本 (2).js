var useid;
function getGUIDCallback(guid){
	useid=guid;
}
function checkForm(){
	//校验文本是否为空
	if ($("#feedbackTextarea").val() ==""){
			$("#check_kong").html("问题描述不能为空").show();
			$("#feedbackTextarea").focus();
			return false;
		}
	
	if ($("#questionEmail").val() ==""){
			$("#check_kong").html("邮箱内容不能为空").show();
			questionEmail.focus();
			return false;
		}
	//验证邮箱
	if(!(/^([a-zA-Z0-9_-])+@([a-zA-Z0-9_-])+(.[a-zA-Z0-9_-])+/.test($("#questionEmail").val()))) {
		$("#check_kong").html("输入邮箱格式错误").show();
		questionEmail.focus();
		return false;
	}
 	
}
 function MouseWheel(e) {
    alert(1);
         e = e || window.event;
         e.preventDefault();
       
 
        //其他代码
    }
	//"type1":["id1","id2"],
var array_type2id={
	"type1":["id1"],
	"type2":["id2"],
	"type3":["id3"],
	"type4":["id4"],
	"type5":["id5"],
	"type6":["id6"],
	"type7":["id7"],
	"type8":["id8"],
	"type9":["id9"]
};
var array_id2question={
	"id1":"1问题",
	"id2":"2问题",
	"id3":"3问题",
	"id4":"4问题",
	"id5":"5问题",
	"id6":"6问题",
	"id7":"7问题",
	"id8":"8问题",
	"id9":"9问题",
	"id10":"10问题"
};

var arrayScrollMove={
	"id1":["0","0"],
	"id2":["-0.69270833rem","0.265625rem"],
	"id3":["-1.36979167rem","0.515625rem"],
	"id4":["-2.0833333rem","0.76041667rem"],
	"id5":["-2.77125rem","1.015625rem"],
	"id6":["-3.40479167rem","1.26041667rem"],
	"id7":["-3.58333333rem","1.29166667rem"],
	"id8":["-3.49479167rem","1.29166667rem"],
	"id9":["-3.49479167rem","1.29166667rem"],
	"id10":["-3.49479167rem","1.29166667rem"]
};








var array_popid=[];
var array_popid_uniq=[];//拆为单个数组
var new_popid=[];//去重数组
var qBox_hasOn=0;
var scroll_flag=0;

(function () {
	var faqscroll=new scrolls('tabFaq');
	var floatBox_h=$(".floatBox").height();
	var	 popscroll=new scrolls('pop');
	$(".feedTtile a").click(function(e){//意见反馈 title tab切换
			var feedIndex=$(this).index();
			$(this).removeClass("wt").addClass("yj").siblings().removeClass("yj").addClass("wt");
			$(".mainCon").addClass("hide");
			$("#feed"+feedIndex).removeClass("hide");
			$(".qBox").removeClass("on");
			$(".info_scroll_bar2").css("top","0");
			$("#tabFaq_shower").css("top","0");
			$("#feed1 a").removeClass("colorBlue");
			array_popid=[];
			array_popid_uniq=[];//拆为单个数组
			new_popid=[];//去重数组
			qBox_hasOn=0;
			$("#pop_shower").empty();
		})//feedTtile

	$(".qBox").click(function(e){//意见反馈 左侧问题类型选择按钮
		e.stopPropagation();
		$("#pop_shower").html("");
		var qBoxid=$(this).attr("id");
		var last_id=array_type2id[qBoxid];//最后选中的几个id
		
		array_popid_uniq=array_popid.concat(last_id);
		for(var i=0;i<array_popid_uniq.length;i++) {
		　　var items=array_popid_uniq[i];
		　　if($.inArray(items,new_popid)==-1) {
		　　　　new_popid.push(items);
		　　}
		}
		
		
			//alert(last_id+"last");
		/*$("#pop_shower a").each(function() {
			
			//alert($(this).attr("data-id"));
			
			if($.inArray($(this).attr("data-id"),last_id)>0){
				//alert($(this).attr("data-id")+"qqq")
				$(this).addClass("colorBlue");
				}
            
        });		*/
		
		
		//alert(last_id+"last");
		if($(this).hasClass("on")){
			$(this).removeClass("on");
			qBox_hasOn--;
			for(var k=0; k<last_id.length;k++){
				new_popid.splice($.inArray(last_id[k],new_popid),1);
			}
			$("#pop_shower").css("top","0" );
			$(".info_scroll_bar").css("top","0");
		}else{
			$(this).addClass("on");
			qBox_hasOn++;
			
		}	
		//alert(new_popid);
		for(var j=0; j<new_popid.length; j++){
			var html="<a  data-id='"+new_popid[j]+"'><span></span>"+array_id2question[new_popid[j]]+"</a>"
			$("#pop_shower").prepend(html);
			}
		
		if(qBox_hasOn>0){
			$("#floatFAQ").removeClass("hide");
			//alert(floatBox_h);
			//alert($("#pop_shower").height());
			if($("#pop_shower").height()>floatBox_h){
				
				
				$(".info_scroller").removeClass("hide");
				scroll_flag=1;
				
			}else{
				$(".info_scroller").addClass("hide");
				popscroll.stopmousemove();
				scroll_flag=0;
				//$("#pop_shower").css("top","0 !important ");
				//alert($("#pop_shower").height()+"后");
				}
		
		}else{
			$("#floatFAQ").addClass("hide");
			$(".qBox").removeClass("on");
			
			}
		
		$("#pop_shower a").click(function(){
			$(this).parents("#floatFAQ").addClass("hide");
			var feed_blue=$(this).attr("data-id");
			$("#feed0").addClass("hide");
			$("#feed1").removeClass("hide").find("#"+feed_blue).addClass("colorBlue").siblings().removeClass("colorBlue");
			$(".info_scroll_bar2").css("top",arrayScrollMove[feed_blue][1]);
			$("#tabFaq_shower").css("top",arrayScrollMove[feed_blue][0]);
			$("#yj").removeClass("yj").addClass("wt");
			$("#wt").removeClass("wt").addClass("yj");
			$("tabFaq_shower a").addClass("colorBlue");
			
		
		})//floatFAQ
		/*if($("#floatFAQ a").length==0){
			$(".qBox").removeClass("on");
			$("#floatFAQ").addClass("hide");
			}*/
		
	})//qBox
	
   $(document).click(function(e){
		if(!$(e.target).parents("#floatFAQ").length){//点击鼠标关闭意见反馈弹框
			$("#floatFAQ").addClass("hide");
		}
		if(!$(e.target).parent(".contact").length){//点击鼠标关闭意见反馈表单提示
			$("#check_kong").hide();
		}
	});
	
	$(document).keypress(function(){//按下键盘关闭意见反馈表单提示
			$("#check_kong").hide();
	})
		
		
		
	
	
	
	
	
	
	
	
	
	
	
})()

