

$.ajaxSetup({
  data: {csrfmiddlewaretoken: $("#csrf_token").val() },
});


var array_type=[];			//所有选中问题类型列表
var faq_list=[];			//显示在FAQ下的问题列表（去重数组）
var lastType_id=[];			//最后选择类型的问题列表

var qBox_hasOn=0;
var scroll_flag=0;

var array_type2id={
	"type1":["id1"],
	"type2":["id2"],
	"type3":["id3"]
	
};
var array_id2question={
	"id1":"初始化卡住",
	"id2":"应用闪退",
	"id3":"屏幕花屏或黑屏"
	
};

var urls="/bs/";
function make_signature(signiture_array){
	var timestamp=new Date().getTime();
	signiture_array.sort();
	var params = signiture_array.join("");
	var signature = $.md5(params+timestamp.toString());
	var result = {};
	result.timestamp = timestamp;
	result.signature = signature;
	//alert(result.signature);
	return result;
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
			$("#questionEmail").focus();
			return false;
		}
	//验证邮箱
	if(!(/^([a-zA-Z0-9_-])+@([a-zA-Z0-9_-])+(.[a-zA-Z0-9_-])+/.test($("#questionEmail").val()))) {
		$("#check_kong").html("输入邮箱格式错误").show();
		$("#questionEmail").focus();
		return false;
	}
 	var data_des=[];
	$(".qwrap a").each(function() {
        if($(this).hasClass("on")){
			data_des.push($(this).attr("data-des"));
		}
    });
	$("#submit_type").attr("value",data_des);
     var result = make_signature(["email","content","type","guid"]);
     $.post(urls+"feedback",
	  {
		content:$("#feedbackTextarea").val(),
		guid:browser2Client('getguid','',''),
		//guid:"djklj",
		email:$("#questionEmail").val(),
		type:$("#submit_type").val(),
		signature:result.signature,
		timestamp:result.timestamp
	  },
	  function(data,status){
		console.log(data.success);
		if(data.success){
			//console.log(1);
			//$("#check_kong").html("意见提交成功").show();
		}else {
			if(data.message=="post in 1 min"){
				$("#check_kong").html("反馈过于频繁，请等待1分钟后提交。").show();
			}else if(data.message=="do not post duplicate content"){
				$("#check_kong").html("您已提交过相同意见").show();
			}else{
				$("#check_kong").html("意见提交失败").show();
			}
		}
	  });	
		
	browser2Client('feedback',$('#questionEmail').val(),$('#feedbackTextarea').val());	
}


function checkForm2(){
	//校验文本是否为空
	if ($("#detail_biaoti").val() ==""){
			$("#check_kong2").html("标题不能为空").show();
			$("#detail_biaoti").focus();
			return false;
			//event.preventDefault(); 与return false作用相同阻止默认行为表单提交
		}
 	 var result = make_signature(["level","app_center_id","content","title","guid"]);
     $.post(urls+"rating_comment",
	  {
		level:$("#level").val(),
		guid:"ee",
		/*guid:browser2Client('getguid','',''),*/
		app_center_id:$("#app_center_id").val(),
		title:$("#detail_biaoti").val(),
		content:$("#detail_miaoshu").val(),
		signature:result.signature,
		timestamp:result.timestamp
	  },
	  function(data,status){
		if(data.success){
			//console.log(1);
			$("#check_kong2").html("评论添加成功").show();
			$("#detail_biaoti").val("");
			$("#detail_miaoshu").val("");
			var _hmtl;
			_html="<div class='xiaobian_tit'>"+data.result.title+"</div>";
			_html+="<div class='xiaobian_con'><span>匿名</span>——<span>"+data.result.uptime+"</span></div>";
			_html+="<div class='xiaobian_con'>"+data.result.content+"</div>";
			$(".pincon2").prepend(_html);
		}else{
			if(data.message=="more than 50 times"){
				$("#check_kong2").html("您已超出评论数量上限").show();
			}else{
				$("#check_kong2").html("添加评论失败").show();
				}
			
			}
	  });	
		
		
}
function qd_CH(platform){
		var qudao;
		if(platform){
			switch (platform){
				case "game9":
					 qudao="九游";
					 break;
				case "360":
					 qudao="360";
					 break;
				case "official":
					 qudao="官方";
					 break;
				case "baidu":
					 qudao="百度";
					 break;
				case "wandoujia":
					 qudao="豌豆荚";
					 break;
				case "xiaomi":
					 qudao="小米";
					 break;
				case "2345":
					 qudao="2345";
					 break;
				case "guopan":
					 qudao="果盘";
					 break;
				case "lianxiang":
					 qudao="联想";
					 break;
				case "huawei":
					 qudao="华为";
					 break;
				case "yiwan":
					 qudao="益玩";
					 break;
				case "37wan":
					 qudao="37玩";
					 break;
				case "other":
					 qudao="其他";
					 break;
			}
		
	 	} 
		return qudao;
	}




function homeInit(){
		var center_url = urls+"app_center_home";
		var array_picture = [{0:'stars_0'},{0.5:'stars_1'},{1:'stars_2'},{1.5:'stars_3'},{2:'stars_4'}];
		$.get(center_url,function(data,status){ 
			//appcenter推荐游戏函数
			function get_recommend_game(box,recommend_game){
				var i,item1;
				var renm;
				switch (recommend_game){
					case "selected_game":
					renm="精品游戏";
					break;
					case "latest_game":
					renm="新品游戏";
					break;
					case "online_game_recommend":
					renm="网游推荐";
					break;
					case "console_game_recommend":
					renm="单机推荐";
					break;
					case "app_recommend":
					renm="应用推荐";
					break;
				}
				$.each(data.result[recommend_game],function(i,item1){
					var score=item1['level'];
					var _html1='';
					var star_array=[];
					var star_pic='';
					for(var d=0;  d<5; d++){
						if(score>2){
							star_array.push(2);
							score-=2;
						}else{
							star_array.push(score);
							score=0;
						}
					}
					
					_html1+="<dl><dt><a onclick=\"browser2Client('showdetails','app_detail_html?id="+item1['id']+"','');_hmt.push([\'_trackEvent\', \'首页_"+renm+"_"+(i+1)+"\',\'click\'])\"> <div class='mask_white'></div>";
					if(item1['icon_tag']){
						var hanzi;
						switch (item1['icon_tag']){
							case "chong_fan":
								 hanzi="充返";
								 break;
							case "shou_fa":
								 hanzi="首发";
								 break;
							case "re_men":
								 hanzi="热门";
								 break;
							}
						_html1+="<div class='app_tag'><img src='../../static/images/appcenter/"+item1['icon_tag']+".png'><span>"+hanzi+"</span></div>";
				    }
					if(item1['icon_url']==null||item1['icon_url']==""||item1['icon_url']=="None"){
						var arr_down=['unknown',item1['title'],item1['quick_download_download_url']];
					}else{
						var arr_down=[item1['icon_url'],item1['title'],item1['quick_download_download_url']];
					}
					_html1+="</a><div class='az_btn' onclick=\"browser2Client(\'installapp\','"+arr_down+"','"+item1['quick_download_package_name']+"');_hmt.push([\'_trackEvent\', \'首页_"+renm+"_"+(i+1)+"_一键安装\',\'click\'])\">一键安装</div>";
					_html1+="<img src='"+item1['icon_url']+"'></dt>";
					_html1+="<a onclick=\"browser2Client('showdetails','app_detail_html?id="+item1['id']+"','');_hmt.push([\'_trackEvent\', \'首页_"+renm+"_"+(i+1)+"\',\'click\'])\"> <dd class='app_name'>"+item1['title']+"</dd></a>";
					_html1+="<dd class='app_star'>";
					
					$.each(star_array,function(){
						if (this <= 0)
						{
							star_pic = array_picture[0][0];							
						}
						else if (this <= 0.5)
						{
							star_pic = array_picture[1][0.5];
						}
						else if (this < 1.5)
						{
							star_pic = array_picture[2][1];
						}
						else if (this < 2)
						{
							star_pic = array_picture[3][1.5];
						}
						else if (this >= 2)
						{
							star_pic = array_picture[4][2];
						}
						_html1+="<img src='../../static/images/appcenter/"+star_pic+".png'>";
					});
					
					_html1+="</dd><dd class='app_type'>";
					if(item1['type']!="" && item1['type'] instanceof Array){
						$.each(item1['type'],function(){
							_html1+="<a href='app_center_tag_html?tag="+this+"'><span>"+this+"</span></a>"
						})
					}
					_html1+="</dd></dl>";
					box.append(_html1);
					});
		  }
		  //appcenter排行榜函数
		  function get_applist(box,list_name){
				var i,item1;
				var renm;
				var _html3='';
				switch (list_name){
					case "hot_list":
					renm="热门榜";
					break;
					case "sell_well_list":
					renm="畅销榜";
					break;
					case "app_list":
					renm="应用榜";
					break;
				}
				$.each(data.result[list_name],function(i,item1){
					var nums=i+1;
					_html3+="<dl><a onclick=\"browser2Client('showdetails','app_detail_html?id="+item1['id']+"','');_hmt.push([\'_trackEvent\', \'首页_"+renm+"_"+(i+1)+"\',\'click\'])\"> <dt><img src='";
					if(item1['small_icon_url']==""||item1['small_icon_url']==null||item1['small_icon_url']=="null"){
						_html3+=item1['icon_url']+"'> </dt><dd class='app_name'>"+item1['title']+"</dd></a>";
					}else{
						_html3+=item1['small_icon_url']+"'> </dt><dd class='app_name'>"+item1['title']+"</dd></a>";
					}
					_html3+="<dd class='app_type'>";
					if(item1['type']!="" && item1['type'] instanceof Array){
						$.each(item1['type'],function(){
							_html3+="<a href='app_center_tag_html?tag="+this+"'><span>"+this+"</span></a>";
						})
					}
					_html3+="</dd><dd class='app_down'><span>"+item1['download_count']+"</span>+次下载</dd>";
					if(item1['icon_url']==null||item1['icon_url']==""||item1['icon_url']=="None"||item1['small_icon_url']==null||item1['small_icon_url']==""||item1['small_icon_url']=="None"){
						var arr_down=['unknown',item1['title'],item1['quick_download_download_url']];
					}else{
						var arr_down=[item1['icon_url'],item1['title'],item1['quick_download_download_url']];
					}
					_html3+="<dd class='downloadbtn'><a onclick=\"browser2Client(\'installapp\','"+arr_down+"','"+item1['quick_download_package_name']+"');_hmt.push([\'_trackEvent\', \'首页_"+renm+"_下载按钮_"+(i+1)+"\',\'click\'])\">下载</a></dd>";
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
		   //appcenter头部轮播
		  function get_toploop(box,list_name){
				var i,item1;
				var _html3='';
				$.each(data.result[list_name],function(i,item1){
					_html3+="<li>";
					if(item1['topic_game']){
						var arr_down=[item1['topic_game'].icon_url,item1['topic_game'].game_name,item1['topic_game'].download_url];
						_html3+="<a onclick=\"browser2Client('installapp','"+arr_down+"','"+item1['topic_game'].package_name+"');_hmt.push([\'_trackEvent\', \'首页_轮播图_"+(i+1)+"\',\'click\'])\"><img src='"+item1['small_image_url']+"'></a>";	
					}else{
						if(item1['url']=="None"||item1['url']==null){
							_html3+="<a href='app_center_topic_detail_html?topic_id="+item1['id']+"&topic_name="+item1['topic_name']+"' onclick=\"_hmt.push([\'_trackEvent\', \'首页_轮播图_"+(i+1)+"\',\'click\'])\"><img src='"+item1['small_image_url']+"'></a>";	
						}else{
							_html3+="<a href='"+item1['url']+"' onclick=\"_hmt.push([\'_trackEvent\', \'首页_轮播图_"+(i+1)+"\',\'click\'])\"><img src='"+item1['small_image_url']+"'></a>";	
						}
					}
						
					_html3+="</li>";
					//box.append(_html3);
				});
				box.hide();
				box.html(_html3);
				var num = box.find("img").length;
				box.find("img").load(function() {
					num--;
					if (num <= 0) {
						box.show();
					}
				})
			}
		  //appcenter底部专题
		  function get_bottomzt(box,list_name){
				var i,item1;
				$.each(data.result[list_name],function(i,item1){
					var _html3='';
					if(item1['topic_game']){
						var arr_down=[item1['topic_game'].icon_url,item1['topic_game'].game_name,item1['topic_game'].download_url];
						
						_html3+="<dl class='app_huodong'><a onclick=\"browser2Client('installapp','"+arr_down+"','"+item1['topic_game'].package_name+"');_hmt.push([\'_trackEvent\', \'首页_轮播图_"+(i+1)+"\',\'click\'])\"><div class='mask_white'></div><dt><img src='"+item1['small_image_url']+"'></dt></a>";	
					}else{
						if(item1['url']=="None"||item1['url']==null){
							_html3+="<dl class='app_huodong'><a href='app_center_topic_detail_html?topic_id="+item1['id']+"&topic_name="+item1['topic_name']+"'onclick=\"_hmt.push([\'_trackEvent\', \'首页_专题_"+(i+1)+"\',\'click\'])\"><div class='mask_white'></div>";
						}else{
							_html3+="<dl class='app_huodong'><a href='"+item1['url']+"'onclick=\"_hmt.push([\'_trackEvent\', \'首页_专题_"+(i+1)+"\',\'click\'])\"><div class='mask_white'></div>";
						}
						_html3+="<dt><img src='"+item1['small_image_url']+"'></dt></a>";	
					}
					
					_html3+="</dl>";
					box.append(_html3);
				});
			}
			//appcenter推荐默认
			get_recommend_game($(".jp_carousel_con"),"selected_game");
			carouselPic("jp_carousel",6);
			get_recommend_game($(".xp_carousel_con"),"latest_game");
			carouselPic("xp_carousel",6);
			//appcenter排行榜默认
			get_applist($(".app_listbox"),"hot_list");
			
			//appcenter排行榜tab切换
			$(".app_bangdan a").click(function(){
				$(this).addClass("on").siblings().removeClass("on");
				_html3='';
				var list_name=$(this).attr("data-list");
			    get_applist($(".app_listbox"),list_name);
		    })
			//appcenter底部专题
			get_bottomzt($("#hd_carousel_con"),"zhuanti");
			carouselPic("hd_carousel",3);
			
			//appcenter头部轮播
			get_toploop($("#app_kvshower"),"lunbo");
			var app_kv= new Zoompic("app_kv");
			$(".app_kv_scrumb1,.app_kv_scrumb2,.app_kv_scrumb3").css({"background":"none","box-shadow":"none"});
			window.onresize = function (){
				app_kv.resizes();
				clearTimeout(timer_loop);
				var timer_loop = setTimeout(function() {
					appmainscroll.resize_scroll();
				}, 10);
			}

		})
	
	}
	
function recommendInit(){
	   
		var center_url = urls+"app_center_recommend";
		var array_picture = [{0:'stars_0'},{0.5:'stars_1'},{1:'stars_2'},{1.5:'stars_3'},{2:'stars_4'}];
		$.get(center_url,function(data,status){ 
			//apprecommend推荐分类戏函数
			function get_recommend_category(box,recommend_category){
				var i,item1;
				var renm;
				switch (recommend_category){
					case "selected_game":
					renm="精品游戏";
					break;
					case "latest_game":
					renm="新品游戏";
					break;
					case "online_game_recommend":
					renm="网游推荐";
					break;
					case "console_game_recommend":
					renm="单机推荐";
					break;
					case "app_recommend":
					renm="应用推荐";
					break;
				}
				$.each(data.result[recommend_category],function(i,item1){
					var score=item1['level'];
					var _html1='';
					var star_array=[];
					var star_pic='';
					for(var d=0;  d<5; d++){
						if(score>2){
							star_array.push(2);
							score-=2;
						}else{
							star_array.push(score);
							score=0;
						}
					}
					
					_html1+="<dl><dt><a onclick=\"browser2Client('showdetails','app_detail_html?id="+item1['id']+"','');_hmt.push(['_trackEvent', '推荐_"+renm+"_"+(i+1)+"','click'])\"><div class='mask_white'></div> ";
					if(item1['icon_tag']){
						var hanzi;
						switch (item1['icon_tag']){
							case "chong_fan":
								 hanzi="充返";
								 break;
							case "shou_fa":
								 hanzi="首发";
								 break;
							case "re_men":
								 hanzi="热门";
								 break;
							}
						_html1+="<div class='app_tag'><img src='../../static/images/appcenter/"+item1['icon_tag']+".png'><span>"+hanzi+"</span></div>";
						}
						
					if(item1['icon_url']==null||item1['icon_url']==""||item1['icon_url']=="None"){
						var arr_down=['unknown',item1['title'],item1['quick_download_download_url']];
					}else{
						var arr_down=[item1['icon_url'],item1['title'],item1['quick_download_download_url']];
					}
					_html1+="</a><div class='az_btn' onclick=\"browser2Client(\'installapp\','"+arr_down+"','"+item1['quick_download_package_name']+"');_hmt.push([\'_trackEvent\', \'推荐_"+renm+"_"+(i+1)+"_一键安装\',\'click\'])\">一键安装</div>";
					_html1+="<img src='"+item1['icon_url']+"'></dt>";
					_html1+="<a onclick=\"browser2Client('showdetails','app_detail_html?id="+item1['id']+"','');_hmt.push(['_trackEvent', '推荐_"+renm+"_"+(i+1)+"','click'])\"><dd class='app_name'>"+item1['title']+"</dd></a>";
					_html1+="<dd class='app_star'>";
					
					$.each(star_array,function(){
						if (this <= 0)
						{
							star_pic = array_picture[0][0];							
						}
						else if (this <= 0.5)
						{
							star_pic = array_picture[1][0.5];
						}
						else if (this < 1.5)
						{
							star_pic = array_picture[2][1];
						}
						else if (this < 2)
						{
							star_pic = array_picture[3][1.5];
						}				else if (this >= 2)
						{
							star_pic = array_picture[4][2];
						}
						_html1+="<img src='../../static/images/appcenter/"+star_pic+".png'>";
					});
					
					_html1+="</dd><dd class='app_type'>";
					if(item1['type']!="" && item1['type'] instanceof Array){
						$.each(item1['type'],function(){
							_html1+="<a href='app_center_tag_html?tag="+this+"'><span>"+this+"</span></a>"
						})
					}
					
					_html1+="</dd><dd class='app_down'><span>"+item1['download_count']+"</span>+次下载</dd></dl>";
					box.append(_html1);
					});
		  }			
		  	//recommend分类游戏
			get_recommend_category($(".wangyou"),"online_game_recommend");
			carouselPic("wangyoubox",9);
			get_recommend_category($(".danji"),"console_game_recommend");
			carouselPic("danjibox",9);
			get_recommend_category($(".yingyong"),"app_recommend");
			carouselPic("yingyongbox",9);
			window.onresize = function (){
			clearTimeout(timer_loop);
			var timer_loop = setTimeout(function() {
			appmainscroll.resize_scroll();
			}, 10);
		};
		})
		
}

function listInit(){
		var center_url = urls+"app_center_board";
		$.get(center_url,function(data,status){ 
			
		  //applist排行榜函数
		 
		  function get_applist(box,list_name){
				var i,item1;
				var renm;
				var _html3='';
				switch (list_name){
					case "hot_list":
					renm="热门榜";
					break;
					case "sell_well_list":
					renm="畅销榜";
					break;
					case "app_list":
					renm="应用榜";
					break;
				
				}
				$.each(data.result[list_name],function(i,item1){
					var nums=i+1;
					
					_html3+="<dl><a onclick=\"browser2Client('showdetails','app_detail_html?id="+item1['id']+"','');_hmt.push(['_trackEvent', '排行榜_"+renm+"_"+(i+1)+"','click'])\"> <dt><img src='";
					if(item1['small_icon_url']==""||item1['small_icon_url']==null||item1['small_icon_url']=="null"){
						_html3+=item1['icon_url']+"'> </dt><dd class='app_name'>"+item1['title']+"</dd></a>";
					}else{
						_html3+=item1['small_icon_url']+"'> </dt><dd class='app_name'>"+item1['title']+"</dd></a>";
					}
					
					_html3+="<dd class='app_type'>";
					if(item1['type']!="" && item1['type'] instanceof Array){
						$.each(item1['type'],function(){
							_html3+="<a href='app_center_tag_html?tag="+this+"'><span>"+this+"</span></a>"
						})
					}
					_html3+="</dd><dd class='app_down'><span>"+item1['download_count']+"</span>+次下载</dd>";
					
					if(item1['icon_url']==null||item1['icon_url']==""||item1['icon_url']=="None"||item1['small_icon_url']==null||item1['small_icon_url']==""||item1['small_icon_url']=="None"){
						var arr_down=['unknown',item1['title'],item1['quick_download_download_url']];
					}else{
						var arr_down=[item1['icon_url'],item1['title'],item1['quick_download_download_url']];
					}
					_html3+="<dd class='downloadbtn'><a onclick=\"browser2Client(\'installapp\','"+arr_down+"','"+item1['quick_download_package_name']+"');_hmt.push([\'_trackEvent\', \'排行榜_"+renm+"_下载按钮_"+(i+1)+"\',\'click\'])\">下载</a></dd>";
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
			//applist排行榜
			get_applist($("#applistcon_hot"),"hot_list");
			get_applist($("#applistcon_well"),"sell_well_list");
			get_applist($("#applistcon_app"),"app_list");
			var applist=new scrolls('applist');
			
			window.onresize = function (){
				clearTimeout(timer_loop);
				var timer_loop = setTimeout(function() {
				applist.resize_scroll();
				}, 10);
		    };
			if($("#applist_container").height()>=$("#applist_shower").height()){
				$("#applist_scroller").hide();
				}
		})
		
	
	}
	
function topicInit(){
	var center_url = urls+"app_center_topic_list";
	$.get(center_url,function(data,status){ 
	
	   //apptopic头部轮播
	  function get_toploop(box){
			var i,item1;
			$.each(data.result.lunbo,function(i,item1){
				var _html3='';
				_html3+="<li>";
				if(item1['topic_game']){
					var arr_down=[item1['topic_game'].icon_url,item1['topic_game'].game_name,item1['topic_game'].download_url];
					_html3+="<a onclick=\"browser2Client('installapp','"+arr_down+"','"+item1['topic_game'].package_name+"');_hmt.push([\'_trackEvent\', \'首页_轮播图_"+(i+1)+"\',\'click\'])\"><img src='"+item1['small_image_url']+"'></a>";	
				}else{		
					if(item1['url']=="None"||item1['url']==null){
						_html3+="<a href='app_center_topic_detail_html?topic_id="+item1['id']+"'  onclick=\"_hmt.push([\'_trackEvent\', \'专题_轮播图_"+(i+1)+"\',\'click\'])\"><img src='"+item1['small_image_url']+"'></a>";	
					}else{
						_html3+="<a href='"+item1['url']+"'  onclick=\"_hmt.push([\'_trackEvent\', \'专题_轮播图_"+(i+1)+"\',\'click\'])\"><img src='"+item1['small_image_url']+"'></a>";	
					}
				}
				_html3+="</li>";
				box.append(_html3);
			});
		}
	  //apptopic底部专题
	  function get_bottomzt(box){
			var i,item1;
			$.each(data.result.zhuanti,function(i,item1){
				var _html3='';
				if(item1['topic_game']){
					var arr_down=[item1['topic_game'].icon_url,item1['topic_game'].game_name,item1['topic_game'].download_url];
					_html3+="<dl class='app_huodong'><a onclick=\"browser2Client('installapp','"+arr_down+"','"+item1['topic_game'].package_name+"');_hmt.push([\'_trackEvent\', \'首页_轮播图_"+(i+1)+"\',\'click\'])\"><div class='mask_white'></div><dt><img src='"+item1['small_image_url']+"'></dt></a>";	
				}else{						
					if(item1['url']=="None"||item1['url']==null){
						_html3+="<dl class='app_huodong'><a href='app_center_topic_detail_html?topic_id="+item1['id']+"'  onclick=\"_hmt.push([\'_trackEvent\', \'专题_精彩专题_"+(i+1)+"\',\'click\'])\"><div class='mask_white'></div>";
					}else{
						_html3+="<dl class='app_huodong'><a href='"+item1['url']+"'  onclick=\"_hmt.push([\'_trackEvent\', \'专题_精彩专题_"+(i+1)+"\',\'click\'])\"><div class='mask_white'></div>";
					}
					_html3+="<dt><img src='"+item1['small_image_url']+"'></a></dt>";	
				}
				
				_html3+="</dl>";
				box.append(_html3);
			});
		}
		//apptopic头部轮播
		get_toploop($("#app_kvshower2"));
		//apptopic页面底部活动
		get_bottomzt($("#topic_carousel_con"));
		var app_kv2= new Zoompic("app_kv2");
		window.onresize = function (){
			app_kv2.resizes();
		};
		carouselPic("topic_carousel",3);
		
	})
	
}

function topicDetailInit(box){
	var center_url = urls+"app_center_topic_list";
	$.get(center_url,function(data,status){ 
	  //apptopic专题列表
	  function get_topicdetail(box){
			var i,item1,_html3='';
			$.each(data.result.lunbo,function(i,item1){
				_html3+="<dl class='app_huodong'><a href='app_center_topic_detail_html?topic_id="+item1['id']+"' onclick=\"_hmt.push(['_trackEvent', '专题全部_"+(i+1)+"','click'])\"><div class='mask_white'></div>";
				_html3+="<dt><img src='"+item1['small_image_url']+"'></a></dt>";	
				_html3+="</dl>";
			});
			box.html(_html3);
			$.each(data.result.zhuanti,function(i,item1){
				
				_html3+="<dl class='app_huodong'><a href='app_center_topic_detail_html?topic_id="+item1['id']+"' onclick=\"_hmt.push(['_trackEvent', '专题全部_"+(i+1)+"','click'])\"><div class='mask_white'></div>";
				_html3+="<dt><img src='"+item1['small_image_url']+"'></a></dt>";	
				_html3+="</dl>";
			});
			box.hide();
			box.html(_html3);
			var num = box.find("img").length;
			box.find("img").load(function() {
				num--;
				if (num <= 0) {
					box.show();
					if($("#apprecommend_container").height()>=$("#apprecommend_shower").height()){
						$("#apprecommend_scroller").hide();
					}
				}
			})
			
		}
		
		get_topicdetail($("#jczt"));
		
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
			
			$("#feed1 a").removeClass("colorBlue");
			$("#feed1").find(".dot").css("background","#fff");
			$("#tabFaq_scroll_bar").css("top",0);
			$("#tabFaq_shower").css("top",0);
			qBox_hasOn=0;
			$("#pop_shower").empty();
		});//feedTtile
	
	$(".gdetailtit a").click(function(){//意见反馈 title tab切换
			var feedIndex=$(this).index();
			$(this).removeClass("wt").addClass("yj").siblings().removeClass("yj").addClass("wt");
			$(".appdetail_box").addClass("hide");
			$(".detailtab"+feedIndex).removeClass("hide");
			$("#tabFaq_shower").css("top",0);
			$(".info_scroll_bar2").css("top",0);
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
			var html="<a  data-id='"+faq_list[j]+"' href='#"+faq_list[j]+"'><span></span>"+array_id2question[faq_list[j]]+"</a>"
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
		var arrayScrollMove={
			"id1":["-12.67rem","1.3rem"],
			"id2":["-12.67rem","1.3rem"],//第一个参数为shower移动距离，第二个参数为bar移动距离
			"id3":["-12.67rem","1.3rem"]
		};

		$("#pop_shower a").click(function(){
			$(this).parents("#floatFAQ").addClass("hide");
			var feed_blue=$(this).attr("data-id");
			$("#feed0").addClass("hide");
			$("#feed1").removeClass("hide").find("#"+feed_blue).addClass("colorBlue").siblings().removeClass("colorBlue");
			$("#feed1").removeClass("hide").find("#"+feed_blue).find(".dot").css("background","#008bef").parent().siblings().find(".dot").css("background","#fff");
			var hh=$("#tabFaq_scroller").height()-$(".info_scroll_bar2").height();
			var hh2=$("#tabFaq_shower").height()-$("#tabFaq_container").height();
			$("#tabFaq_scroll_bar").css("top",hh+"px");
			$("#tabFaq_shower").css("top",-hh2+"px");
			//$(".info_scroll_bar2").css("top",arrayScrollMove[feed_blue][1]);
			//$("#tabFaq_shower").css("top",arrayScrollMove[feed_blue][0]);
			$("#yj").removeClass("yj").addClass("wt");
			$("#wt").removeClass("wt").addClass("yj");
			$("tabFaq_shower a").addClass("colorBlue");
		
		})//floatFAQ
		
		
	})//qBox
	$(".fanhui").click(function(){
			$("#wt").removeClass("yj").addClass("wt");
			$("#yj").removeClass("wt").addClass("yj");
			$("#feed1").addClass("hide");
			$("#feed0").removeClass("hide");
			faq_list = [];
			array_type=[];
			$("#pop_shower").html("");
			$(".qBox").removeClass("on");
			$("#feed1 a").removeClass("colorBlue");
			qBox_hasOn=0;
			$("#pop_shower").empty();
	})
   $(document).click(function(e){
		if(!$(e.target).parents("#floatFAQ").length){//点击鼠标关闭意见反馈弹框
			$("#floatFAQ").addClass("hide");
		}
		if(!$(e.target).parent(".check_kong").length&&!$(e.target).parent(".questionSubmit").length){//点击鼠标关闭意见反馈表单提示
			$("#check_kong").hide();
		}
		if(!$(e.target).parent(".check_kong2").length&&!$(e.target).parent(".detail_tj").length){//点击鼠标关闭意见反馈表单提示
			$("#check_kong2").hide();
		}
		
	});
	
	$(document).keypress(function(){//按下键盘关闭意见反馈表单提示
			$("#check_kong").hide();
			$("#check_kong2").hide();
	})
	
	
	//分类tab切换
	

	

	

	
	
	
})()