<?php
$this->breadcrumbs=array(
	'User Cleaners'=>array('index'),
	$model->name,
);

$this->menu=array(
	array('label'=>'列表 UserCleaner','url'=>array('index')),
	array('label'=>'添加 UserCleaner','url'=>array('create')),
	array('label'=>'编辑 UserCleaner','url'=>array('update','id'=>$model->id)),
);
?>

<?php $this->widget('bootstrap.widgets.TbDetailView',array(
	'data'=>$model,
	'attributes'=>array(
		'id',
		'user_id',
		'cleaner_id',
		'name',
		'point_x',
		'point_y',
		'city',
		'add_time',
		'wifi_name',
		'wifi_pwd',
	),
)); ?>
