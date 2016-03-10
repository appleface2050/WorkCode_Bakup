<div class="sortable row-fluid ui-sortable">
	<a data-rel="tooltip" class="well span3 top-block" href="<?php echo $this->createUrl('/admin/user'); ?>">
		<span class="icon32 icon-blue icon-users"></span>
		<div>用户总数</div>
		<div><?php echo User::model()->calculateTotal(); ?></div>
	</a>

	<a data-rel="tooltip" class="well span3 top-block" href="<?php echo $this->createUrl('/admin/online'); ?>">
		<span class="icon32 icon-blue icon-calendar"></span>
		<div>在线净化器总数</div>
		<div><?php echo Yii::app()->redis->sCard(EKeys::getAllonlineKey()); ?></div>
	</a>

	<a data-rel="tooltip" class="well span3 top-block" href="<?php echo $this->createUrl('/admin/active'); ?>">
		<span class="icon32 icon-blue icon-contacts"></span>
		<div>当前活跃用户数</div>
		<div><?php echo Yii::app()->redis->sCard(EKeys::getActiveUserKey()); ?></div>
	</a>
	
	<a data-rel="tooltip" class="well span3 top-block" href="<?php echo $this->createUrl('/admin/default'); ?>">
		<span class="icon32 icon-volume-on"></span>
		<div></div>
		<div></div>
	</a>
</div>