<?php
$this->breadcrumbs=array(
	'Cleaner Statuses'=>array('index'),
	$model->id,
);

$this->menu=array(
	array('label'=>'列表 CleanerStatus','url'=>array('index')),
	array('label'=>'添加 CleanerStatus','url'=>array('create')),
	array('label'=>'编辑 CleanerStatus','url'=>array('update','id'=>$model->id)),
);
?>

<?php $this->widget('bootstrap.widgets.TbDetailView',array(
	'data'=>$model,
	'attributes'=>array(
		'id',
		'qrcode',
		'first_use_time',
		'filter_surplus_life',
		'city',
		'level',
		'childlock',
		'status',
		'timeset',
		'automatic',
		'operator_uid',
		'aqi',
		'update_time',
		'silence',
		'silence_start',
		'silence_end',
		'point_x',
		'point_y',
		'switch',
		'type',
		'voc',
		'version',
	),
)); ?>
