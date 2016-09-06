
(function () {

	var ParseOrientation = function () {
        /// <summary>解析方向</summary>
        /// <field name="designWidth" type="Number">设计宽度</field>
        /// <field name="htmlWidth" type="Number">网页实际宽度</field>
        /// <field name="htmlHeight" type="Number">网页实际高度</field>
        /// <field name="ratioWidth" type="Number">网页实际宽度同设计宽度比</field>
        /// <field name="isOpenForceWindow" type="Bool">是否开启强制横竖屏</field>
        var html = document.documentElement;
        this.designWidth = 1920;
        this.htmlWidth = html.offsetWidth;
        this.htmlHeight = html.offsetHeight;
        this.ratioWidth = html.clientWidth / this.designWidth;
		
        this.isOpenForceWindow = false;
        html = null;
		//alert(ratioWidth);
    };
    ParseOrientation.prototype.isOrientation = function () {
        /// <summary>是否是竖屏</summary>
        return this.htmlWidth < this.htmlHeight;
    };
    ParseOrientation.prototype.getWindowWidth = function () {
        /// <summary>获取当前窗口宽度</summary>
        var self = this;
        var html = document.documentElement;
        self.htmlWidth = html.clientWidth;
        self.htmlHeight = html.clientHeight;
        if (self.isOrientation()) {
            return self.htmlHeight;
        } else {
            return self.htmlWidth;
        }
    };
    ParseOrientation.prototype.change = function () {
        /// <summary>窗口改变后计算</summary>
        var self = this;
        /// <var type="Element">当前页面</var>
        var html = document.documentElement;
        /// <var type="Number">当前窗口物理分辨率和显示清晰度</var>
        var dpr = Math.round(window.devicePixelRatio || 1);
        /// <var type="Element">获取meta标签</var>
        var metaEl = document.querySelector('meta[name="viewport"]');
        /// <var type="Element">获取style标签</var>
        var fontEl = document.querySelector("#styleHtml");
        if (!metaEl) {
            metaEl = document.createElement("meta");
            metaEl.setAttribute("name", "viewport");
        };
        if (!fontEl) {
            fontEl = document.createElement('style');
            fontEl.id = "styleHtml";
            html.firstElementChild.appendChild(fontEl);
        };
        //正常比例显示页面
        metaEl.setAttribute('content', 'initial-scale=1,maximum-scale=1, minimum-scale=1,user-scalable=no');
        //写入高清倍数
        html.setAttribute('data-dpr', dpr);
        //动态写入样式
        fontEl.innerHTML = 'html{font-size:' + (self.getWindowWidth() / 10) + 'px!important;}';
        //强制横竖屏
        if (self.isOpenForceWindow) {
            if (!self.isOrientation()) {
                //已经是竖屏
                document.body.style.cssText += "width:" + self.htmlWidth + "px;height:" + self.htmlHeight + "px;-webkit-transform: rotate(0);transform: rotate(0);";
            } else {
                //横屏
                var x = -((self.htmlHeight - self.htmlWidth) / 2);
                document.body.style.cssText += "width:" + self.htmlHeight + "px;height:" + self.htmlWidth + "px;-webkit-transform: rotate(-90deg) translate(" + x + "px," + x + "px);transform: rotate(-90deg) translate(" + x + "px," + x + "px);";
            }
        };
        dpr = html = metaEl = self = fontEl = null;
    };
    var windowRatio = new ParseOrientation();
    (function () {
        var handle;
        window.addEventListener("resize", function () {
            cancelAnimationFrame(handle);
            handle = requestAnimationFrame(function () {
                windowRatio.change();
            });
        }, false);
		
		
    })();
    windowRatio.change();
	var dl_length=$("#jp_carousel_con").find("dl").length; 
	var dl_margin=($("#jp_carousel_con").find("dl").css("margin-left")).split("p"); 
	var dl_width=$("#jp_carousel_con").find("dl").width();
	$("#jp_carousel_con,#xp_carousel_con").width((dl_width+dl_margin[0]*2)*dl_length+10);
	
	var dl_length2=$("#hd_carousel_con").find("dl").length; 
	var dl_margin2=($("#hd_carousel_con").find("dl").css("margin-left")).split("p"); 
	var dl_width2=$("#hd_carousel_con").find("dl").width();
	$("#hd_carousel_con").width((dl_width2+dl_margin2[0]*2)*dl_length2+10);


})();

(function ($) {
	$.fn.extend({
		"carouselPic": function (obj,offset,bool) {
		 	var dl_length=$(this).find("dl").length; 
			var dl_margin=($(this).find("dl").css("margin-left")).split("p"); 
			var dl_width=$(this).find("dl").width();
			$("#"+obj+"_con").width((dl_width+dl_margin[0]*2)*dl_length+10);
			var loopnum1=1;
			var loop_width1;
			var loop_width_end;
			var loop_length;
			var offset;
			var n=1;
				$("#"+obj).bind({
					 mouseover:function(){
						 if(loopnum1==1){
							 $(this).find(".arrow_right").show();
							  $(this).find(".arrow_left").show();
						  }
						
						  if(bool==true){
							  for(var i=0;i<dl_length;i++){
								  if(i%6==0){
						  				$(this).find("dl:eq("+i+")").find(".mask_white").css({"height":"100%","opacity":"0.5"}).show();
										//$(this).find("dl:eq(0)").find(".mask_white").hide();
								  }
							  }
						  }else{
							  for(var i=0;i<dl_length;i++){
								  if(i%3==0){
							  		 $(this).find("dl:eq("+i+")").find(".mask_white").show();
									 //$(this).find("dl:eq(0)").find(".mask_white").hide();
								  }
						}  
					 }
					 },
					 mouseout:function(){
						if(loopnum1==1){
							 $(this).find(".arrow_right").hide();
						}
						  if(bool==true){
							  for(var i=0;i<dl_length;i++){
								  if(i%6==0){
					      		$(this).find("dl:eq("+i+")").find(".mask_white").css({"height":"0.9583333rem","opacity":"0.3"}).hide();
								$(this).find("dl:eq(0)").find(".mask_white").hide();
								  }}
						  }else{
							   for(var i=0;i<dl_length;i++){
								  if(i%3==0){
							  $(this).find("dl:eq("+i+")").find(".mask_white").hide();
							  $(this).find("dl:eq(0)").find(".mask_white").hide();
							  
								  }}}
					 } 
				
				})
			
			
			
			/*switch(loopnum1){
				case 1:
				 $("#"+obj).bind({
					 mouseover:function(){
						  $(this).find(".arrow_right").show();
						  $(this).find("dl:eq(6)").find(".mask_white").css("height","100%").show();
					 },
					 mouseout:function(){
						  $(this).find(".arrow_right").hide();
					      $(this).find("dl:eq(6)").find(".mask_white").css("height","0.9583333rem").hide();
					 } 
				  });
				  break;
				  case dl_length:
				 $("#"+obj).bind({
					 mouseover:function(){
						  $(this).find(".arrow_left").show();
						  $(this).find("dl:eq(6)").find(".mask_white").css("height","100%").show();
					 },
					 mouseout:function(){
						  $(this).find(".arrow_left").hide();
					      $(this).find("dl:eq(6)").find(".mask_white").css("height","0.9583333rem").hide();
					 } 
				  });
				  break;
				  default:
				  $("#"+obj).bind({
					 mouseover:function(){
						  $(this).find(".arrow_left").css("height","100%").show();
						  $(this).find(".arrow_right").css("height","100%").show();
						  //$(this).find("dl:eq(6)").find(".mask_white").show();
					 },
					 mouseout:function(){
						  $(this).find(".arrow_left").css("height","0.9583333rem").hide();
						   $(this).find(".arrow_right").css("height","0.9583333rem").hide();
					 } 
				  });
				  
				  
				  
			}*/
		   $("#"+obj+" dt").bind({
			 mouseover:function(){ $(this).parent().find(".mask_white").show();},
			 mouseout:function(){ $(this).parent().find(".mask_white").hide();} 
			  
			  
			  })
			 $("#"+obj+" .app_name").bind({
			 mouseover:function(){ $(this).parent().find(".mask_white").show();},
			 mouseout:function(){ $(this).parent().find(".mask_white").hide();} 
			  
			  
			  })
		
			$("#"+obj).find(".arrow_left").bind({
				
				/* mouseover:function(){ 
				 	$("#"+obj+"_con").stop(true,true).animate({left:-(offset-0.1)+"rem"},300);
				 },
				 mouseout:function(){ 
				 	$("#"+obj+"_con").stop(true,true).animate({left:-offset+"rem"},300);
				 }, */
				 click:function(){
					 
					 // alert(loopnum1);
					if(bool==true){
					 	loop_width1=(dl_width+dl_margin[0]*2)*6;
						loop_length=Math.ceil(dl_length/6);
						
					}else{
						loop_length=Math.ceil(dl_length/3);
						loop_width1=(dl_width+dl_margin[0]*2)*3; 
						
						}
						
					if((dl_length>6&&bool==true)||(dl_length>3&&bool==false)){
						if(loopnum1<=1){
							//alert("ww");
							  $("#"+obj+"_con").stop(true,true).animate({left:0},500); 
							  loopnum1=1;
						 }else{
							// alert("ee");
							  $("#"+obj+"_con").stop(true,true).animate({left:-(loop_width1*(loopnum1-2))+"px"},500); 
							  loopnum1--;
						 }
					 
				 }}
			})
			
			$("#"+obj).find(".arrow_right").bind({
				
				/* mouseover:function(){ 
				 	$("#"+obj+"_con").stop(true,true).animate({left:-(offset+0.1)+"rem"},300);
				 },
				 mouseout:function(){ 
				 	$("#"+obj+"_con").stop(true,true).animate({left:-offset+"rem"},300);
				 }, */
				 click:function(){
					 
					 //alert(loopnum1);
					//alert(dl_length);
					 if(bool==true){
						 loop_width1=(dl_width+dl_margin[0]*2)*6;
						 loop_length=Math.ceil(dl_length/6);
						
					 }else{
						 loop_width1=(dl_width+dl_margin[0]*2)*3;
						 loop_length=Math.ceil(dl_length/3);
						 
					 }
					if((dl_length>6&&bool==true)||(dl_length>3&&bool==false)){
					 if(loopnum1>=loop_length){
						//alert("aa");
						 
						  $("#"+obj+"_con").stop(true,true).animate({left:-(loop_width1*(loop_length-1))+"px"},500); 
						 loopnum1=loop_length; 
					 }else{
						//alert("bb");
						
						  $("#"+obj+"_con").stop(true,true).animate({left:-(loop_width1*loopnum1)+"px"},500);
						 loopnum1++; 
					 }
					 
				
				 }
				
				 }
				
				
			})
				
			
			
			/*$(window).resize(function(){
				var dl_length=$("#"+obj+"_con").find("dl").length; 
				var dl_margin=($("#"+obj+"_con").find("dl").css("margin-left")).split("p"); 
				var dl_width=$("#"+obj+"_con").find("dl").width();
				$("#"+obj+"_con").width((dl_width+dl_margin[0]*2)*dl_length+10);
				
				//console.log(obj+" "+dl_length+" "+dl_margin[0]*2+" "+dl_width);
				})*/
			
			
			
			  
		}
		
	});
})(jQuery);

function Zoompic()
    {
       this.initialize.apply(this,arguments);
    }
    Zoompic.prototype=
    {
        initialize:function(id)
        {
			
           var _this=this;
           this.box=document.getElementById(id);
           this.oPre=document.getElementById('pre');
           this.oNext=document.getElementById('next');
           this.oUl=document.getElementById('app_kvshower');
           this.aLi=document.getElementById('app_kvshower').getElementsByTagName('li');
           this.timer=null;
           this.iCenter=2;
           this.aStor=[];
           this.options = [
               {width:570, height:300, top:40, left:200, zIndex:1},
                //{width:130, height:170, top:61, left:0, zIndex:2},
                {width:680, height:360, top:20, left:0, zIndex:3},
                {width:760, height:400, top:0, left:300, zIndex:4},
                {width:680, height:360, top:20, left:690, zIndex:3},
                //{width:130, height:170, top:61, left:620, zIndex:2},
                {width:570, height:300, top:40, left:600, zIndex:1}
				 /*{width:120, height:150, top:71, left:0, zIndex:1},
				{width:250, height:200, top:61, left:134, zIndex:2},
                {width:380, height:300, top:0, left:262, zIndex:3},
				{width:250, height:200, top:71, left:496, zIndex:2}
                {width:120, height:150, top:61, left:620, zIndex:1}*/
            ];
			
           for(var i=0;i<this.aLi.length;i++)
			{
				//
				this.aStor[i]=this.aLi[i]; //放入图片
			}
           this.up();
            this._oNext=function()
            {	//alert(aStor);
                return _this.doNext.apply(_this);
            }
			
            this._oPre=function()
            {
                return _this.doPre.apply(_this);
            }
           this.addBing(this.oNext,"click",this._oNext);
           this.addBing(this.oPre,"click",this._oPre);
           setInterval(this._oNext,3000);
        },
		
        doNext:function()
        {alert(1);
          this.aStor.unshift(this.aStor.pop());
          this.up();
        },
        doPre:function()
        {alert(2);
          this.aStor.push(this.aStor.shift());
          this.up();
        },
        up:function()
        {
			_this=this;
			for(var i=0;i<this.aStor.length;i++) 
		    {
				this.oUl.appendChild(this.aStor[i]);
			}
			//alert(this.aStor.length)
			//alert(_this.iCenter)
            for(var i=0;i<this.aStor.length;i++)
            {
               this.aStor[i].index=i;
               if(i<5)
               {
                   this.css(this.aStor[i],"display","block");
                   this.starMove(this.aStor[i],this.options[i],function()
                   {
                       _this.starMove(_this.aStor[_this.iCenter].getElementsByTagName('img')[0],{opacity:100},function()
                       {
                        /*   _this.aStor[_this.iCenter].onmouseover=function()
                           {
                               _this.starMove(this.getElementsByTagName('div')[0],{bottom:0});
                           }
                           _this.aStor[_this.iCenter].onmouseout=function()
                           {
                               _this.starMove(this.getElementsByTagName('div')[0],{bottom:-100});
                           }*/
                       });
                   })
               }
			   else
			   {
					this.css(this.aStor[i],"display","none");
			   }
               
            }
        },
        starMove:function(obj,json,fnEnd)
        {
            _this=this;
            clearInterval(obj.timer);
            obj.timer=setInterval(function()
            {
                var oStop=true;
                for( var attr in json)
                {
                    if(attr=="opacity")
                    {
                        icurr=Math.round(parseFloat(_this.css(obj,attr))*100);
                    }else
                    {
                        icurr=parseInt(_this.css(obj,attr));
                    }
                   var ispeed=(json[attr]-icurr)/8;
                    ispeed=ispeed>0?Math.ceil(ispeed):Math.floor(ispeed);
                    if(icurr!=json[attr])
                    {
                        oStop=false;
                        _this.css(obj,attr,icurr+ispeed);
                    }
                }
                if(oStop)
                {
                    clearInterval(obj.timer);
                    fnEnd && fnEnd.apply(_this,arguments);
                }
            },20);
        },
        css:function(obj,attr,value)
        {
         if(arguments.length===2)
         {
             return obj.currentStyle?obj.currentStyle[attr]:getComputedStyle(obj,false)[attr];
         }else if(arguments.length===3)
         {
           switch (attr)
           {
               case "width":
               case "height":
               case "left":
               case "right":
               case "top":
               case "bottom":
                   obj.style[attr]=value+'px';
                   break;
               case "opacity":
                   //obj.style.filter="alpha(opacity="+value+")";
                  // obj.style.opacity=value/100;
                   break;
               default :
                   obj.style[attr]=value;
                   break;
           }
         }
        },
        addBing:function(obj,type,fnEnd)
        {
            return obj.addEventListener?obj.addEventListener(type,fnEnd,false):obj.attachEvent("on"+type,fnEnd);
        }
    }







































