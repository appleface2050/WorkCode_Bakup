/*$.ajaxSetup({
  data: {csrfmiddlewaretoken: $("#csrf_token").val() },
});*/
var window_h;
var main_h
function check_w_height(){
	window_h=$(window).height();
	main_h=$(".mainbox").height();
	if ((navigator.userAgent.match(/(phone|pad|pod|iPhone|iPod|ios|iPad|Android|Mobile|BlackBerry|IEMobile|MQQBrowser|JUC|Fennec|wOSBrowser|BrowserNG|WebOS|Symbian|Windows Phone)/i))) {
		if(main_h<window_h){
			$(".mainbox").height($(window).height()*1.05);
		}
	}
}
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
function checkForm2(){
	//校验文本是否为空
	if ($("#detail_biaoti").val() ==""){
			$(".check_kong2").html("标题不能为空").show();
			$("#detail_biaoti").focus();
	 		 return false;
	}
	 var result = make_signature(["level","app_center_id","content","title","guid"]);
     $.post(urls+"rating_comment",
	  {
		level:$("#level").val(),
		guid:"web",
		app_center_id:$("#app_center_id").val(),
		title:$("#detail_biaoti").val(),
		content:$("#detail_miaoshu").val(),
		signature:result.signature,
		timestamp:result.timestamp
	  },
	  function(data,status){
		if(data.success){
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



var index;
var total_page
var jsondata;
var stop=true;
function SearchApp(obj){
	$(".gamebox").html("");
	index=1;
	getsearch(obj);
	check_w_height();
}
function getsearch(obj){
	stop=true;
	//console.log("index1:"+index);
	if(index<=total_page){
		$('.jzz span').show();
	}else{
		$('.jzz span').hide();
	}
	
	var result = make_signature(["query","pageNum","resNum","guid"]);

	$.ajax({
		type: 'GET',
		url: "/bs/search" ,
		data: { "query":obj,
				"guid":"123-webbrowser",
				"pageNum":index,    //页数
				"resNum":"20",       //每页数据条数
				"signature":result.signature,
				"timestamp":result.timestamp
			   },
		success:function(data) {
			if(data.result.search_result.length>0 ){  
				total_page=data.result.total_page;
				//console.log("total_page"+data.result.total_page);
					$(".loading").hide();
					
					if(index<=total_page){
						index++;
							for(var i=0; i<data.result.search_result.length;i++){
								//console.log(data.result.search_result[i].game_name+"——次序"+i);
								var htmlStr="";
								htmlStr+="<div class='searchbox' ><dl >";
								if(data.result.search_result[i].platform_name=="百度"){
									 htmlStr+="<dt ><a target='_blank'  href='app_detail_html?cid="+data.result.search_result[i].cid+"' onclick=\"_hmt.push([\'_trackEvent\', \'搜索游戏\',\'click\'])\"><div class='search_shade'></div><img src='"+data.result.search_result[i].icon_url+"'></a></dt>";
									htmlStr+="<dd class='app_name'><a  target='_blank' href='app_detail_html?cid="+data.result.search_result[i].cid+"'  onclick=\"_hmt.push([\'_trackEvent\', \'搜索游戏\',\'click\'])\"  title='"+data.result.search_result[i].game_name+"'>"+data.result.search_result[i].game_name+"</a></dd>";
									htmlStr+="<dd class='app_qd'><a  target='_blank' href='app_detail_html?cid="+data.result.search_result[i].cid+"'  onclick=\"_hmt.push([\'_trackEvent\', \'搜索游戏\',\'click\'])\" >"+data.result.search_result[i].platform_name+"</a></dd>";
									if(data.result.search_result[i].download_count){
										htmlStr+="<dd class='app_down' ><a  target='_blank' href='app_detail_html?cid="+data.result.search_result[i].cid+"'  onclick=\"_hmt.push([\'_trackEvent\', \'搜索游戏\',\'click\'])\" ><span>"+data.result.search_result[i].download_count+"</span>+次下载</a></dd>";
									}else{
										htmlStr+="<dd class='app_down'><a  target='_blank' href='app_detail_html?cid="+data.result.search_result[i].cid+"'  onclick=\"_hmt.push([\'_trackEvent\', \'搜索游戏\',\'click\'])\" ><span></span></a></dd>";
										}
									htmlStr+="<dd class='app_size' ><a  target='_blank' href='app_detail_html?cid="+data.result.search_result[i].cid+"'  onclick=\"_hmt.push([\'_trackEvent\', \'搜索游戏\',\'click\'])\" >"+data.result.search_result[i].size+"</a></dd>";
									htmlStr+="<dd class='downloadbtn'><a  href='"+data.result.search_result[i].download_url+"' onclick=\"_hmt.push([\'_trackEvent\', \'搜索_安装按钮\',\'click\'])\">安装</a></dd>";
									htmlStr+="</dl><a  target='_blank' href='app_detail_html?cid="+data.result.search_result[i].cid+"'  onclick=\"_hmt.push([\'_trackEvent\', \'搜索游戏\',\'click\'])\"><p class='search_des'>"+data.result.search_result[i].instruction+"</p></a></div>";
								}else if(data.result.search_result[i].platform_name=="total_360"){
									htmlStr+="<dt ><a  target='_blank' href='app_detail_html?total_360_id="+data.result.search_result[i].id+"' onclick=\"_hmt.push([\'_trackEvent\', \'搜索游戏\',\'click\'])\"><div class='search_shade'></div><img src='"+data.result.search_result[i].icon_url+"'></a></dt>";
									htmlStr+="<dd class='app_name' ><a  target='_blank' href='app_detail_html?total_360_id="+data.result.search_result[i].id+"' onclick=\"_hmt.push([\'_trackEvent\', \'搜索游戏\',\'click\'])\"  title='"+data.result.search_result[i].game_name+"'>"+data.result.search_result[i].game_name+"</a></dd>";
									htmlStr+="<dd class='app_qd'><a  target='_blank'  href='app_detail_html?total_360_id="+data.result.search_result[i].id+"' onclick=\"_hmt.push([\'_trackEvent\', \'搜索游戏\',\'click\'])\" >360</a></dd>";
									if(data.result.search_result[i].download_count){
										htmlStr+="<dd class='app_down'><a  target='_blank' href='app_detail_html?total_360_id="+data.result.search_result[i].id+"' onclick=\"_hmt.push([\'_trackEvent\', \'搜索游戏\',\'click\'])\" ><span>"+data.result.search_result[i].download_count+"</span>+次下载</a></dd>";
									}else{
										htmlStr+="<dd class='app_down' ><a  target='_blank' href='app_detail_html?total_360_id="+data.result.search_result[i].id+"' onclick=\"_hmt.push([\'_trackEvent\', \'搜索游戏\',\'click\'])\" ><span></span></a></dd>";
										}
									htmlStr+="<dd class='app_size' > <a  target='_blank' href='app_detail_html?total_360_id="+data.result.search_result[i].id+"' onclick=\"_hmt.push([\'_trackEvent\', \'搜索游戏\',\'click\'])\" >"+data.result.search_result[i].size+"</a></dd>";
									htmlStr+="<dd class='downloadbtn'><a href='"+data.result.search_result[i].download_url+"' onclick=\"_hmt.push([\'_trackEvent\', \'搜索_安装按钮\',\'click\'])\">安装</a></dd>";
									htmlStr+="</dl><a  target='_blank' href='app_detail_html?total_360_id="+data.result.search_result[i].id+"' onclick=\"_hmt.push([\'_trackEvent\', \'搜索游戏\',\'click\'])\" ><p class='search_des'>"+data.result.search_result[i].instruction+"</p></a></div>";
								}else{
									htmlStr+="<dt ><a  target='_blank' href='app_detail_html?game_library_id="+data.result.search_result[i].id+"' onclick=\"_hmt.push([\'_trackEvent\', \'搜索游戏\',\'click\'])\"><div class='search_shade'></div><img src='"+data.result.search_result[i].icon_url+"'></a></dt>";
									htmlStr+="<dd class='app_name' ><a  target='_blank' href='app_detail_html?game_library_id="+data.result.search_result[i].id+"' onclick=\"_hmt.push([\'_trackEvent\', \'搜索游戏\',\'click\'])\"  title='"+data.result.search_result[i].game_name+"'>"+data.result.search_result[i].game_name+"</a></dd>";
									htmlStr+="<dd class='app_qd'><a  target='_blank'  href='app_detail_html?game_library_id="+data.result.search_result[i].id+"' onclick=\"_hmt.push([\'_trackEvent\', \'搜索游戏\',\'click\'])\" >"+qd_CH(data.result.search_result[i].platform_name)+"</a></dd>";
									if(data.result.search_result[i].download_count){
										htmlStr+="<dd class='app_down'><a  target='_blank' href='app_detail_html?game_library_id="+data.result.search_result[i].id+"' onclick=\"_hmt.push([\'_trackEvent\', \'搜索游戏\',\'click\'])\" ><span>"+data.result.search_result[i].download_count+"</span>+次下载</a></dd>";
									}else{
										htmlStr+="<dd class='app_down' ><a  target='_blank' href='app_detail_html?game_library_id="+data.result.search_result[i].id+"' onclick=\"_hmt.push([\'_trackEvent\', \'搜索游戏\',\'click\'])\" ><span></span></a></dd>";
										}
									htmlStr+="<dd class='app_size' > <a  target='_blank' href='app_detail_html?game_library_id="+data.result.search_result[i].id+"' onclick=\"_hmt.push([\'_trackEvent\', \'搜索游戏\',\'click\'])\" >"+data.result.search_result[i].size+"</a></dd>";
									htmlStr+="<dd class='downloadbtn'><a href='"+data.result.search_result[i].download_url+"' onclick=\"_hmt.push([\'_trackEvent\', \'搜索_安装按钮\',\'click\'])\">安装</a></dd>";
									htmlStr+="</dl><a  target='_blank' href='app_detail_html?game_library_id="+data.result.search_result[i].id+"' onclick=\"_hmt.push([\'_trackEvent\', \'搜索游戏\',\'click\'])\" ><p class='search_des'>"+data.result.search_result[i].instruction+"</p></a></div>";
								}	
								$('.gamebox').append(htmlStr);
							}
				}
			}else{  
				  var search_fail="<p class='search_fail'>抱歉，没有找到您搜索的相关应用</p>";
				  $('.jzz span').hide();
				  $(".loading").hide();
				  $('.gamebox').html(search_fail);
			}  
		} 
		
	});
	
}


var urls="/bs/";
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
					
					_html1+="<dl><dt><a  target='_blank' href='app_detail_html?id="+item1['id']+"' onclick=\"_hmt.push([\'_trackEvent\', \'首页_"+renm+"_"+(i+1)+"\',\'click\'])\"> <div class='mask_white'></div>";
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
					_html1+="</a><a class='az_btn' href='"+item1['quick_download_download_url']+"' onclick=\"_hmt.push([\'_trackEvent\', \'首页_"+renm+"_"+(i+1)+"_一键下载\',\'click\'])\">一键下载</a>";
					
					_html1+="<img src='"+item1['icon_url']+"'></dt>";
					
					
					_html1+="<dd class='app_name'><a  target='_blank' href='app_detail_html?id="+item1['id']+"' onclick=\"_hmt.push([\'_trackEvent\', \'首页_"+renm+"_"+(i+1)+"\',\'click\'])\"> "+item1['title']+"</a></dd>";
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
						_html1+="<img src='../../static/images/browser_appcenter/"+star_pic+".png'>";
					});
					
					_html1+="</dd><dd class='app_type'>";
					if(item1['type']!="" && item1['type'] instanceof Array){
						$.each(item1['type'],function(){
							_html1+="<a href='app_center_tag_html?tag="+this+"'><span>"+this+"</span></a>"
						})
					}
					//_html1+=item1['id'];
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
					_html3+="<dl> <dt><a  target='_blank' href='app_detail_html?id="+item1['id']+"'  onclick=\"_hmt.push([\'_trackEvent\', \'首页_"+renm+"_"+(i+1)+"\',\'click\'])\"><img src='";
					if(item1['small_icon_url']==""||item1['small_icon_url']==null||item1['small_icon_url']=="null"){
						_html3+=item1['icon_url']+"'></a> </dt><dd class='app_name'><a  target='_blank' href='app_detail_html?id="+item1['id']+"'  onclick=\"_hmt.push([\'_trackEvent\', \'首页_"+renm+"_"+(i+1)+"\',\'click\'])\">"+item1['title']+"</a></dd>";
					}else{
						_html3+=item1['small_icon_url']+"'> </a></dt><dd class='app_name'><a  target='_blank' href='app_detail_html?id="+item1['id']+"'  onclick=\"_hmt.push([\'_trackEvent\', \'首页_"+renm+"_"+(i+1)+"\',\'click\'])\">"+item1['title']+"</a></dd>";
					}
					_html3+="<dd class='app_type'>";
					if(item1['type']!="" && item1['type'] instanceof Array){
						$.each(item1['type'],function(){
							_html3+="<a href='app_center_tag_html?tag="+this+"'><span>"+this+"</span></a>";
						})
					}
					_html3+="</dd><dd class='app_down'><span>"+item1['download_count']+"</span>+次下载</dd>";
					_html3+="<dd class='downloadbtn'><a href='"+item1['quick_download_download_url']+"' onclick=\"_hmt.push([\'_trackEvent\', \'首页_"+renm+"_"+(i+1)+"\',\'click\'])\">下载</a></dd>";
					_html3+="<dd class='nums'>";
						if(nums>9){
							var a1=parseInt(nums/10);
							var a2=Math.floor(nums%10);
							_html3+="<img src='../../static/images/browser_appcenter/num"+a2+".png'><img src='../../static/images/browser_appcenter/num"+a1+".png'>";	
						}else{
							_html3+="<img src='../../static/images/browser_appcenter/num"+nums+".png'>";	
							}
						
					_html3+="</dd></dl>";
					
				});
				box.html(_html3);
			}
		   //appcenter头部轮播
		  function get_toploop(box,list_name){
				var i,item1;
				//console.log(list_name);
				$.each(data.result[list_name],function(i,item1){
					var _html3='';
					_html3+="<li>";
					if(item1['topic_game']){
						_html3+="<a href='"+item1['topic_game'].download_url+"' onclick=\"_hmt.push([\'_trackEvent\', \'首页_轮播图_"+(i+1)+"\',\'click\'])\"><img src='"+item1['small_image_url']+"'></a>";	
					}else{
						if(item1['url']=="None"||item1['url']==null){
							_html3+="<a href='app_center_topic_detail_html?topic_id="+item1['id']+"&topic_name="+item1['topic_name']+"' onclick=\"_hmt.push([\'_trackEvent\', \'首页_轮播图_"+(i+1)+"\',\'click\'])\"><img src='"+item1['small_image_url']+"'></a>";	
						}else if(item1['url']=="/bs/topic_pokemongo"){
							_html3+="<a href='/bs_browser/topic_pokemongo' onclick=\"_hmt.push([\'_trackEvent\', \'首页_轮播图_"+(i+1)+"\',\'click\'])\"><img src='"+item1['small_image_url']+"'></a>";	
						}else{
							_html3+="<a href='"+item1['url']+"' onclick=\"_hmt.push([\'_trackEvent\', \'首页_轮播图_"+(i+1)+"\',\'click\'])\"><img src='"+item1['small_image_url']+"'></a>";	
						}
					}
						
					_html3+="</li>";
					box.append(_html3);
				});
				
			}
		  //appcenter底部专题
		  function get_bottomzt(box,list_name){
				var i,item1;
				
				$.each(data.result[list_name],function(i,item1){
					var _html3='';
					if(item1['topic_game']){
						_html3+="<dl class='app_huodong'><a href='"+item1['topic_game'].download_url+"' onclick=\"_hmt.push([\'_trackEvent\', \'首页_轮播图_"+(i+1)+"\',\'click\'])\"><div class='mask_white'></div><dt><img src='"+item1['small_image_url']+"'></dt></a>";	
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
			check_w_height();


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
					
					_html1+="<dl><dt><a  target='_blank' href='app_detail_html?id="+item1['id']+"'  onclick=\"_hmt.push(['_trackEvent', '推荐_"+renm+"_"+(i+1)+"','click'])\"><div class='mask_white'></div> ";
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
						
					_html1+="</a><a class='az_btn' href='"+item1['quick_download_download_url']+"' onclick=\"_hmt.push([\'_trackEvent\', \'推荐_"+renm+"_"+(i+1)+"_一键下载\',\'click\'])\">一键下载</a>";
					_html1+="<img src='"+item1['icon_url']+"'></dt>";
					_html1+="<a  target='_blank' href='app_detail_html?id="+item1['id']+"' onclick=\"_hmt.push(['_trackEvent', '推荐_"+renm+"_"+(i+1)+"','click'])\"><dd class='app_name'>"+item1['title']+"</dd></a>";
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
						_html1+="<img src='../../static/images/browser_appcenter/"+star_pic+".png'>";
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
			check_w_height();
		})
		
}

function listInit(){
		var center_url = urls+"app_center_board";
		$.get(center_url,function(data,status){ 
			
		  //applist排行榜函数
		 
		  function get_applist(box,list_name){
				var i,item1;
				var renm;
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
					var _html3='';
					_html3+="<dl><dt><a  target='_blank' href='app_detail_html?id="+item1['id']+"' onclick=\"_hmt.push(['_trackEvent', '排行榜_"+renm+"_"+(i+1)+"','click'])\"> <img src='";
					if(item1['small_icon_url']==""||item1['small_icon_url']==null||item1['small_icon_url']=="null"){
						_html3+=item1['icon_url']+"'></a> </dt> <dd class='app_name'><a  target='_blank' href='app_detail_html?id="+item1['id']+"' onclick=\"_hmt.push(['_trackEvent', '排行榜_"+renm+"_"+(i+1)+"','click'])\">"+item1['title']+"</a></dd>";
					}else{
						_html3+=item1['small_icon_url']+"'></a> </dt><dd class='app_name'><a  target='_blank' href='app_detail_html?id="+item1['id']+"' onclick=\"_hmt.push(['_trackEvent', '排行榜_"+renm+"_"+(i+1)+"','click'])\"> "+item1['title']+"</a></dd>";
					}
					
					_html3+="<dd class='app_type'>";
					if(item1['type']!="" && item1['type'] instanceof Array){
						$.each(item1['type'],function(){
							_html3+="<a href='app_center_tag_html?tag="+this+"'><span>"+this+"</span></a>"
						})
					}
					_html3+="</dd><dd class='app_down'><span>"+item1['download_count']+"</span>+次下载</dd>";
					_html3+="<dd class='downloadbtn'><a href='"+item1['quick_download_download_url']+"' onclick=\"_hmt.push([\'_trackEvent\', \'排行榜_"+renm+"_下载按钮_"+(i+1)+"\',\'click\'])\">下载</a></dd>";
					
					_html3+="<dd class='nums'>";
						if(nums>9){
							var a1=parseInt(nums/10);
							var a2=Math.floor(nums%10);
							_html3+="<img src='../../static/images/browser_appcenter/num"+a2+".png'><img src='../../static/images/browser_appcenter/num"+a1+".png'>";	
						}else{
							
							_html3+="<img src='../../static/images/browser_appcenter/num"+nums+".png'>";	
							}
						
					_html3+="</dd></dl>";
					box.append(_html3);
				});
				
			}
						
			//applist排行榜
			get_applist($("#applistcon_hot"),"hot_list");
			get_applist($("#applistcon_well"),"sell_well_list");
			get_applist($("#applistcon_app"),"app_list");
			check_w_height();

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
		carouselPic("topic_carousel",3);
		
	})
	
}

function topicDetailInit(box){
	var center_url = urls+"app_center_topic_list";
	$.get(center_url,function(data,status){ 
	
	  //专题列表
	  function get_topicdetail(box){
			var i,item1;
			$.each(data.result.lunbo,function(i,item1){
				var _html3='';
				_html3+="<dl class='app_huodong'><dt><a href='app_center_topic_detail_html?topic_id="+item1['id']+"' onclick=\"_hmt.push(['_trackEvent', '专题全部_"+(i+1)+"','click'])\"><div class='mask_white'></div>";
				_html3+="<img src='"+item1['small_image_url']+"'></a></dt>";	
				_html3+="</dl>";
				box.append(_html3);
			});
			
			$.each(data.result.zhuanti,function(i,item1){
				var _html3='';
				_html3+="<dl class='app_huodong'><dt><a href='app_center_topic_detail_html?topic_id="+item1['id']+"' onclick=\"_hmt.push(['_trackEvent', '专题全部_"+(i+1)+"','click'])\"><div class='mask_white'></div>";
				_html3+="<img src='"+item1['small_image_url']+"'></a></dt>";	
				_html3+="</dl>";
				box.append(_html3);
			});
		}
		
		get_topicdetail($("#jczt"));
		check_w_height();
		
	})
	
	
}
function myBrowser(){//判断浏览器类型
    var userAgent = navigator.userAgent; //取得浏览器的userAgent字符串
    var isOpera = userAgent.indexOf("Opera") > -1;
    if (isOpera) {
        return "Opera"
    }; //判断是否Opera浏览器
    if (userAgent.indexOf("Firefox") > -1) {
        return "FF";
    } //判断是否Firefox浏览器
    if (userAgent.indexOf("Chrome") > -1){
  return "Chrome";
 }
    if (userAgent.indexOf("Safari") > -1) {
        return "Safari";
    } //判断是否Safari浏览器
    if (userAgent.indexOf("compatible") > -1 && userAgent.indexOf("MSIE") > -1 && !isOpera) {
        return "IE";
    }; //判断是否IE浏览器
}


var search_val;
$(document).ready(function(){
	
   $(document).click(function(event){
	    var e = event || window.event || arguments.callee.caller.arguments[0];
	    $(".hint").hide();
		var mb = myBrowser();
		if ("IE" == mb||"FF" == mb) {
			if(!$(e.target).parent(".pincon").length){//适用ie,firefox
				$(".check_kong2").hide();
			}
		}else{
			if(!$(e.target).parent(".detail_tj").length){//适用chrome,oepra,safari
				$(".check_kong2").hide();
			}
		}
	});
	
	$(document).keyup(function(event){
		var e = event || window.event || arguments.callee.caller.arguments[0];
			$(".hint").hide();
		    $(".check_kong2").hide();
	})
	$(".search_icon").click(function(){
		if ($(".search_input input").val() ==""){
			$(".hint").show();
			$(".search_input input").focus();
			return false;
			//event.preventDefault(); 与return false作用相同阻止默认行为表单提交
		}else{
			search_val=$(".search_input input").val();
			var exdate=new Date();
			exdate.setDate(exdate.getDate()+1);
			document.cookie="appsearchword="+search_val+";expires="+exdate.toGMTString();
			window.location.href="appsearch_html"; 
		}
	});
	
	$(document).keydown(function(event) {
		var e = event || window.event || arguments.callee.caller.arguments[0];
		if (e && e.keyCode == "13") {//keyCode=13是回车键
			$('.search_icon').click();
		}
	});
	
	$(window).resize(function() {
        check_w_height();
    });

	
	
	
})