var useid;

var array_q=[{"1问题":"1答案"},"2问题","3问题","4问题","5问题","6问题","7问题","8问题"];
var array_a=["1答案","2答案","3答案","4答案","5答案","6答案","7答案","8答案"];

var json_faq=[{"q":"0问题","a":"0答案","id":"q0"},{"q":"1问题","a":"1答案","id":"q1"},{"q":"2问题","a":"2答案","id":"q2"},{"q":"3问题","a":"3答案","id":"q3"},{"q":"4问题","a":"4答案","id":"q4"},{"q":"5问题","a":"5答案","id":"q5"}]

function getGUIDCallback(guid){
	useid=guid;
}

function checkForm(){
	
	//GmApi.getUserGUID("getGUIDCallback");
	//document.getElementById("guid").value=useid;
	//校验文本是否为空
	//alert(document.getElementById("guid").value);
	
	if ($("#feedbackTextarea").val() ==""){
			$("#check_kong").html("问题描述不能为空");
			$("#feedbackTextarea").focus();
			$("#check_kong").removeClass("hide");
			//removeClass(document.querySelector("#check_kong"),"hide");
			return false;
		}
	
	if ($("#questionEmail").val() ==""){
			$("#check_kong").html("邮箱内容不能为空");
			questionEmail.focus();
			$("#check_kong").removeClass("hide");
			//removeClass(document.querySelector("#check_kong"),"hide");
			return false;
		}

	//验证邮箱

	if(!(/^([a-zA-Z0-9_-])+@([a-zA-Z0-9_-])+(.[a-zA-Z0-9_-])+/.test($("#questionEmail").val()))) {
		$("#check_kong").html("输入邮箱格式错误");
		questionEmail.focus();
		$("#check_kong").removeClass("hide");
		return false;
	}
 	
}


(function () {
	$(".feedTtile a").click(function(e){
		var feedIndex=$(this).index();
		$(this).removeClass("wt").addClass("yj").siblings().removeClass("yj").addClass("wt");
		$(".mainCon").addClass("hide");
		$("#feed"+feedIndex).removeClass("hide");
		})//feedTtile
	
	$(".qBox").click(function(e){
	
		e.stopPropagation();
		var qBox_hasOn=0;
		var aray_pop=[];
		var qBox_target=$(e.target).index();
		var qBox_target_on=false;
		if($(this).hasClass("on")){
			$(this).removeClass("on");
		}else{
			$(this).addClass("on");
		}
		$(".qBox").each(function() {
            if($(this).hasClass("on")){
				qBox_hasOn++;
				var qBoxIndex=$(this).index();
				aray_pop.push(qBoxIndex);
				
				
				
				
				
			}
        });
		
		/*if($(e.target).hasClass("on")){
			qBox_target_on=true;
		}else{
			qBox_target_on=false;
		}*/
		//alert(qBox_target_on)
		if(qBox_hasOn>0){
			$("#floatFAQ").removeClass("hide");
		}else{
			$("#floatFAQ").addClass("hide");
			}
		
		
			
		
		
		
		
		
		
		
		
		})//qBox
	
	   $(document).click(function(e){
				
			if(!$(e.target).parents("#floatFAQ").length){
				$("#floatFAQ").addClass("hide");
			}
			if(!$(e.target).parents(".questionSubmit").length){
				$("#check_kong").addClass("hide");
			}
			$(document).keypress(function(){
				$("#check_kong").addClass("hide");
			})
				
		});
	
	
		$("#floatFAQ a").click(function(){
			$(this).parents("#floatFAQ").addClass("hide");
			$("#feed0").addClass("hide");
			$("#feed1").removeClass("hide");
		
		})
	
	
	
	
	
	
	
	
	
	
	
})()

