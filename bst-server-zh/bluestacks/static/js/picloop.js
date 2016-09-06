 function carouselPic (obj,num) {
			var loopnum1=1;
		 	var dl_length=$("#"+obj).find("dl").length;//共有多少方块
			var dl_margin=$("#"+obj).find("dl").css("margin-left").split("px");//方块间距
			var loop_width=$("#"+obj).find("dl").width();//每个方块宽度
			///console.log(dl_margin+"__"+loop_width)
			//alert(loop_width+"normal");
			var loop_width1=Number(dl_margin[0])+Number(loop_width);//每个方块宽+间隙
			//共几屏
			var loop_length=Math.ceil(dl_length/num); 
			//最后一屏的方块个数
			var last_li=dl_length%num;
			//每次移动长度
			var move_length;
		//	console.log(obj+"__"+loopnum1+"__"+dl_length+"__"+$(this).find("dl").css("margin-left"));
			
				$("#"+obj).bind({
				 mouseover:function(){
					if(loopnum1<=1){
						$(this).find(".arrow_right").show();
						$(this).find(".arrow_left").hide();
					}else if(loopnum1>=loop_length){
						$(this).find(".arrow_left").show();
						$(this).find(".arrow_right").hide();
					}else{
						$(this).find(".arrow_right").show();
						$(this).find(".arrow_left").show();
						}
						
				 },
				 mouseout:function(){
					$(this).find(".arrow_right").hide();
					$(this).find(".arrow_left").hide();
				 } 
			
			});
			
		
			$("#"+obj).find(".arrow_left").bind({
				
			     mouseover:function(){
					$("#"+obj+"_con").stop(false,true).animate({left:"+="+(loop_width1/5)},500); 
				 },
				 mouseout:function(){
					$("#"+obj+"_con").stop(false,true).animate({left:"-="+(loop_width1/5)},500); 
				 },
				 click:function(){
					 dl_margin=$("#"+obj).find("dl").css("margin-left").split("px");//方块间距
					loop_width=$("#"+obj).find("dl").width();//每个方块宽度
					///console.log(dl_margin+"__"+loop_width)
					loop_width1=Number(dl_margin[0])+Number(loop_width);//每个方块宽+间隙
					//console.log("now"+loopnum1);
					if(loopnum1==loop_length){	
						 if(last_li==0){
							 move_length=loop_width1*num 
						 }else{
							move_length=loop_width1*last_li
						 }
					 }else{
					   move_length=loop_width1*num 
				     }
					 if(!$("#"+obj+"_con").is(":animated")){
						 $("#"+obj+"_con").stop(false,false).animate({left:"+="+(move_length+"px")},500,function(){
							loopnum1--;	
							$("#"+obj).find(".arrow_right").show();
							//console.log(loopnum1+"left"+loop_length);	
		
							if(loopnum1<=1){
								loopnum1=1;
								$("#"+obj).find(".arrow_left").hide();
							
						     }	
				         });
						 
					}
					 
						
				 }
				 
			});
			
			$("#"+obj).find(".arrow_right").bind({
				 mouseover:function(){
					$("#"+obj+"_con").stop(false,true).animate({left:"-="+(loop_width1/5)},500); 
				 },
				 mouseout:function(){
					$("#"+obj+"_con").stop(false,true).animate({left:"+="+(loop_width1/5)},500); 
				 },
				 click:function(){
					 dl_margin=$("#"+obj).find("dl").css("margin-left").split("px");//方块间距
					loop_width=$("#"+obj).find("dl").width();//每个方块宽度
					///console.log(dl_margin+"__"+loop_width)
					loop_width1=Number(dl_margin[0])+Number(loop_width);//每个方块宽+间隙
					// console.log("now"+loopnum1);
					 if(loopnum1==loop_length-1){	
						 if(last_li==0){
							 move_length=loop_width1*num 
						 }else{
							 move_length=loop_width1*last_li
						 }
					 }else{
					   move_length=loop_width1*num 
				     }
				if(!$("#"+obj+"_con").is(":animated")){
				   $("#"+obj+"_con").stop(false,false).animate({left:"-="+(move_length+"px")},500,function(){
						 loopnum1++; 
						 $("#"+obj).find(".arrow_left").show();
						// console.log(loopnum1+"right"+loop_length);
						 if(loopnum1>=loop_length){
							 loopnum1=loop_length;
					 		$("#"+obj).find(".arrow_right").hide();
						 }
						 
					   })
				   }
				}
				
			});
		
		function resize_w(){
			
			dl_margin=$("#"+obj).find("dl").css("margin-left").split("px");//方块间距
			loop_width=$("#"+obj).find("dl").width();//每个方块宽度
			//console.log(dl_margin[0]+"__"+loop_width)
			loop_width1=Number(dl_margin[0])+Number(loop_width);//每个方块宽+间隙
			//alert(loop_width+"resize"+obj);
			if(loopnum1<=1){
				 $("#"+obj+"_con").css("left",0);
			}else if(loopnum1>=loop_length){
				if(last_li==0){
					$("#"+obj+"_con").css("left",-(loop_width1*num*(loop_length-1))+"px");
					
				}else{
					$("#"+obj+"_con").css("left",-(loop_width1*num*(loopnum1-2)+loop_width1*last_li)+"px");
					}
				
				
			}else{
				 $("#"+obj+"_con").css("left",-(loop_width1*num*(loopnum1-1))+"px");
			}
		}	
		$(window).resize(function(){
			
			clearTimeout(timer_loop);
			var timer_loop = setTimeout(function() {
			resize_w()
			}, 10);
				  
		})
			  
}

/*--------------------------------------------*/

 
 function Zoompic(id){
		var scrumb1_w;
		var scrumb1_h;
		var scrumb1_t;
		
		var scrumb2_w;
		var scrumb2_h;
		var scrumb2_l;
		
		var scrumb3_w;
		var scrumb3_h;
		var scrumb3_t;
		var scrumb3_l;
		var options;
		var timer1;
        this.initialize.apply(this,arguments);
    }
    Zoompic.prototype={
        initialize:function(id){
			var _this=this;
			this.box=typeof id=="string"?document.getElementById(id):id;
			this.oPre=this.box.getElementsByTagName('pre')[0];
			this.oNext=this.box.getElementsByTagName('pre')[1];
			this.oUl=this.box.getElementsByTagName('ul')[0];
			this.aLi=this.oUl.getElementsByTagName('li');
			this.timer=null;
			this.iCenter=1;
			this.aStor=[];                           
			
			for(var i=0;i<this.aLi.length;i++){				
				this.aStor[i]=this.aLi[i]; //放入图片
			}
			this.up();
			this._oNext=function(){
				return _this.doNext.apply(_this);
			}
			this._oPre=function() {
				return _this.doPre.apply(_this);
			}
			   
			this._mouseover=function(){
			   clearInterval(timer1);
			}
			this._mouseout=function(){
				timer1=setInterval(_this._oPre,5000)
			}
			this.re=function(){
				return _this.up.apply(_this);
				}
			timer1=setInterval(_this._oPre,5000);
			this.addBing(this.oNext,"click",this._oPre);
			this.addBing(this.oPre,"click",this._oNext);
			this.addBing(this.box,"mouseover",this._mouseover);
			this.addBing(this.box,"mouseout",this._mouseout);
           
		},
		
		resizes:function(){ 
		   // setTimeout(_this.re,1) ;
			clearTimeout(timer_loop2);
			var timer_loop2 = setTimeout(function() {
				_this.re();
			}, 30);
		},
        doNext:function(){
          this.aStor.unshift(this.aStor.pop());
          this.up();
        },
        doPre:function(){
          this.aStor.push(this.aStor.shift());
          this.up();
        },
        up:function(){ 
			scrumb1_w=document.getElementById("app_kv_left").offsetWidth;
			scrumb1_h=document.getElementById("app_kv_left").offsetHeight;
			scrumb1_t=document.getElementById("app_kv_left").offsetTop;
			
			scrumb2_w=document.getElementById("app_kv_mid").offsetWidth;
			scrumb2_h=document.getElementById("app_kv_mid").offsetHeight;
			scrumb2_l=document.getElementById("app_kv_mid").offsetLeft;
			
			scrumb3_w=document.getElementById("app_kv_right").offsetWidth;
			scrumb3_h=document.getElementById("app_kv_right").offsetHeight;
			scrumb3_t=document.getElementById("app_kv_right").offsetTop;
			scrumb3_l=document.getElementById("app_kv_right").offsetLeft;
			/*console.log("first:"+scrumb1_w+","+scrumb2_w+","+scrumb3_w);
			console.log("firstwindow:"+$("#appmain_container").width());*/
			this.options=[
				{width:scrumb1_w, height:scrumb1_h, top:scrumb1_t, left:0, zIndex:3},
				{width:scrumb2_w, height:scrumb2_h, top:0, left:scrumb2_l, zIndex:4},
				{width:scrumb3_w, height:scrumb3_h, top:scrumb3_t, left:scrumb3_l, zIndex:3},
            ];
			//console.log(this.options)
			_this=this;
			
            for(var i=0;i<this.aStor.length;i++){
              // this.aStor[i].index=i;
               if(i<3){
                   this.css(this.aStor[i],"display","block");
				  // console.log(this.aStor[i]);
                   this.starMove(this.aStor[i],this.options[i])
               }else{
					this.css(this.aStor[i],"display","none");
			   }
            }
        },
        starMove:function(obj,json,fnEnd){
            _this=this;
            clearInterval(obj.timer);
            obj.timer=setInterval(function(){
                var oStop=true;
                for( var attr in json){
                    
                  icurr=parseInt(_this.css(obj,attr));
                    
                   var ispeed=(json[attr]-icurr)/8;
                    ispeed=ispeed>0?Math.ceil(ispeed):Math.floor(ispeed);
                    if(icurr!=json[attr]){
                        oStop=false;
                        _this.css(obj,attr,icurr+ispeed);
                    }
                }
                if(oStop){
                    clearInterval(obj.timer);
                    fnEnd && fnEnd.apply(_this,arguments);
                }
            },1);
        },
        css:function(obj,attr,value){
			if(arguments.length===2){
			// console.log("eeee");
			 return obj.currentStyle?obj.currentStyle[attr]:getComputedStyle(obj,false)[attr];
		 	 }else if(arguments.length===3){
			    // console.log("ggggggg");
				switch (attr){
				   case "width":
				   case "height":
				   case "left":
				   case "right":
				   case "top":
				   case "bottom":
					   obj.style[attr]=value+'px';
					  
					   break;
				 
				   default :
				 //  console.log(obj);
				if(obj.style){
					   obj.style[attr]=value;
			     }
				  break;
									   
				}
			}
        },
        addBing:function(obj,type,fnEnd){
            return obj.addEventListener? obj.addEventListener(type,fnEnd,false):obj.attachEvent("on"+type,fnEnd);
        }
    }






















