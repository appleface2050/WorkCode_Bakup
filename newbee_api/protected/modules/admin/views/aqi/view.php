<?php
$this->breadcrumbs=array(
	'Aqi Indexes'=>array('index'),
	$model->name,
);

$this->menu=array(
	array('label'=>'列表 AqiIndex','url'=>array('index')),
	array('label'=>'添加 AqiIndex','url'=>array('create')),
	array('label'=>'编辑 AqiIndex','url'=>array('update','id'=>$model->city)),
);
?>

<?php $this->widget('bootstrap.widgets.TbDetailView',array(
	'data'=>$model,
	'attributes'=>array(
		'city',
		'name',
		'aqi',
		'update_time',
		'time_point',
	),
)); ?>
