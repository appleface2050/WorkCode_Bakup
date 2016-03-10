<script src="<?php echo Yii::app()->baseUrl;?>/admin/js/bootstrap.min.js"></script>
<script src="<?php echo Yii::app()->baseUrl;?>/admin/js/jquery.uniform.min.js"></script>
<script>
$(function(){
	$('ul.main-menu li a').each(function(){
		if($($(this))[0].href==String(window.location))
			$(this).parent().addClass('active');
	});
})
</script>