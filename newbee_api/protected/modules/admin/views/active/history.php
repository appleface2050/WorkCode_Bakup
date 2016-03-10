<?php
$this->breadcrumbs=array(
	'活跃用户统计'=>array('/admin/active'),
	'活跃历史'
);


Yii::app()->clientScript->registerScript('search', "
$('.search-form form').submit(function(){
	$.fn.yiiGridView.update('active-user-history-grid', {
		data: $(this).serialize()
	});
	return false;
});
");
?>



<?php $this->widget('bootstrap.widgets.TbGridView',array(
	'id'=>'active-user-history-grid',
	'dataProvider'=>$model->search(),
	'type'=>'striped bordered',
	'summaryCssClass' => 'breadcrumb',
	'enableSorting' => false,
	'columns'=>array(
		array('name' => 'user_id', 'header' => '用户', 'value' => '$data->user->nickname'),
		array('header' => '手机号', 'value' => '$data->user->mobile'),
		'date_char',
		array('name' => 'add_time', 'value' => 'date("Y-m-d H:i:s", $data->add_time)'),
	),
)); ?>
