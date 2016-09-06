var useid;

var array_type=[];			//所有选中问题类型列表
var faq_list=[];			//显示在FAQ下的问题列表（去重数组）
var lastType_id=[];			//最后选择类型的问题列表

var qBox_hasOn=0;
var scroll_flag=0;

function getGUIDCallback(guid){
	useid=guid;
}
function checkForm(){
	//校验文本是否为空
	if ($("#feedbackTextarea").val() ==""){
			$("#check_kong").html("问题描述不能为空").show();
			$("#feedbackTextarea").focus();
			return false;
			//event.preventDefault(); 与return false作用相同阻止默认行为表单提交
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
 	var data_des=[];
	$(".qwrap a").each(function() {
        if($(this).hasClass("on")){
			data_des.push($(this).attr("data-des"));
			
			}
    });
	$("#submit_type").attr("value",data_des);

	
}
 function MouseWheel(e) {
         e = e || window.event;
         e.preventDefault();
}

var array_type2id={
	"type1":["id1","id2","id3"],
	"type2":["id2","id3"],
	"type3":["id3","id4"],
	"type4":["id4","id9"],
	"type5":["id5"],
	"type6":["id6","id7"],
	"type7":["id1","id4","id7"],
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
var urls="http://101.201.111.117:9001/bs/";
var selected_game=$(".jp_carousel_con"), _html1='';
var latest_game=$(".xp_carousel_con"), _html2='';
var app_listbox=$(".app_listbox"), _html3='';
function homInit(){
	    //GmApi.getUserGUID("getGUIDCallback");
		var center_url = urls+"app_center_home";
		$.get(center_url,function(data,status){ 
		    
			//推荐游戏函数
			function get_recommend_game(box,recommend_game){
				var i,item1,_html1='';
				$.each(data.result[recommend_game],function(i,item1){
					_html1+="<dl><div class='mask_white'></div><a href='"+urls+"app_detail?id="+item1['id']+"'> <dt><img src='";
					_html1+=item1['icon_url']+"'> </dt><dd class='app_name'>"+item1['title']+"</a>";
					_html1+="<dd class='app_star'><img src='../../static/images/appcenter/star_4.png'></dd><dd class='app_type'>";
					$.each(item1['type'],function(j,tag){
						_html1+="<a href='"+urls+"app_center_type?type="+this+"'><span>"+this+"</a></span>";
						})
					//_html1+=item1['id'];
					_html1+="</dd></dl>";
					
					});
				box.append(_html1);
		  }
			get_recommend_game($(".jp_carousel_con"),"selected_game");
			get_recommend_game($(".xp_carousel_con"),"latest_game");
			
			//排行榜函数
			function get_applist(box,list_name){
				var i,item1,_html3='';
				$.each(data.result[list_name],function(i,item1){
					var nums=item1['order']+1;
					_html3+="<dl><a href='"+urls+"app_detail?id="+item1['id']+"'> <dt><img src='";
					_html3+=item1['icon_url']+"'> </dt><dd class='app_name'>"+item1['title']+"</a>";
					_html3+="<dd class='app_type'>";
					$.each(item1['type'],function(){
						_html3+="<span>"+this+"</span>";
						})
					_html3+="</dd><dd class='app_down'><span>"+item1['download_count']+"</span>次下载</dd>";
					_html3+="<dd class='downloadbtn'><a href='"+urls+"app_detail?id="+item1['id']+"'>下载</a></dd>";
					_html3+="<dd class='nums'>";
						if(nums>9){
							var a1=parseInt(nums/10);
							var a2=Math.floor(nums%10);
							_html3+="<img src='../../static/images/appcenter/num"+a2+".png'><img src='../../static/images/appcenter/num"+a1+".png'>";	
						}else{
							
							_html3+="<img src='../../static/images/appcenter/num"+nums+".png'>";	
							}
						
					_html3+="</dd></dl>";
			
				});
				box.html(_html3);
			}
			//首页排行榜默认
			get_applist($(".app_listbox"),"hot_list");
			
			//排行榜tab切换
			$(".app_bangdan a").click(function(){
				$(this).addClass("on").siblings().removeClass("on");
				_html3='';
				var list_name=$(this).attr("data-list");
			    get_applist($(".app_listbox"),list_name);
		    })
			//排行榜页面
			get_applist($("#applistcon_hot"),"hot_list");
			get_applist($("#applistcon_well"),"sell_well_list");
			get_applist($("#applistcon_app"),"app_list");
			
			
		})
	
	}
(function () {
	
	var floatBox_h=$(".floatBox").height();
	$(".feedTtile a").click(function(e){//意见反馈 title tab切换
			faq_list = [];
			array_type=[];
			$("#pop_shower").html("");
			var feedIndex=$(this).index();
			$(this).removeClass("wt").addClass("yj").siblings().removeClass("yj").addClass("wt");
			$(".mainCon").addClass("hide");
			$("#feed"+feedIndex).removeClass("hide");
			$(".qBox").removeClass("on");
			$(".info_scroll_bar2").css("top","0");
			$("#tabFaq_shower").css("top","0");
			$("#feed1 a").removeClass("colorBlue");
			

			qBox_hasOn=0;
			$("#pop_shower").empty();
		})//feedTtile
			rray_type=[];			//所有选中问题类型列表
			faq_list=[];			//显示在FAQ下的问题列表（去重数组）
			lastType_id=[];	
	$(".qBox").click(function(e){//意见反馈 左侧问题类型选择按钮
		e.stopPropagation();
		$("#pop_shower").html("");
		var qBoxid=$(this).attr("id");
		lastType_id=[];	
		if($(this).hasClass("on")){
			
			$(this).removeClass("on");
			qBox_hasOn--;
			array_type.splice($.inArray(qBoxid,array_type),1);	//取消时，去掉选中类型
			$("#pop_shower").css("top","0" );
			$(".info_scroll_bar").css("top","0");
			//$("#pop_shower a").removeClass("colorBlue");
		}else{
			$(this).addClass("on");
			qBox_hasOn++;
			array_type.push(qBoxid);	
			lastType_id=array_type2id[qBoxid];//最后选中的几个id
				//加入选中类型
		}
		
		// 根据选中类型获取问题列表
		var faq_tmplist = [];	//每次全新统计，先清空
		for(var i=0;i<array_type.length;i++) {
			var curItems = array_type2id[array_type[i]];
			for(var j=0;j<curItems.length;j++) {
				if(($.inArray(curItems[j],faq_tmplist)==-1) && ($.inArray(curItems[j],lastType_id)==-1)) {	//不存在的加入，无需再次去重,lastType_id记录最后的问题，以便显示在最前面
					faq_tmplist.push(curItems[j]);
		　　	}
			}
		}
		faq_list =[];
		faq_list=faq_tmplist.concat(lastType_id);
		//显示问题列表
		for(var j=0; j<faq_list.length; j++){
			var html="<a  data-id='"+faq_list[j]+"'><span></span>"+array_id2question[faq_list[j]]+"</a>"
			$("#pop_shower").prepend(html);
			
			
		}
		//高亮显示最后的问题
	   $("#pop_shower a").each(function() {
				if($.inArray($(this).attr("data-id"),lastType_id)!=-1){
					$(this).addClass("colorBlue");
					}
            
       	}); 	
		if(qBox_hasOn>0){
			$("#floatFAQ").removeClass("hide");
			//alert(floatBox_h);
			//alert($("#pop_shower").height());
			if($("#pop_shower").height()>floatBox_h){				
				$(".info_scroller").removeClass("hide");
				//scroll_flag=1;				
			}else{
				$(".info_scroller").addClass("hide");
				//popscroll.stopmousemove();
				//scroll_flag=0;
				//$("#pop_shower").css("top","0 !important ");
				//alert($("#pop_shower").height()+"后");
				}		
		}else{
			$("#floatFAQ").addClass("hide");
			$(".qBox").removeClass("on");			
		}
			
		$("#pop_shower a").click(function(){
			//alert(1);
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
	
	homInit();
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
})()