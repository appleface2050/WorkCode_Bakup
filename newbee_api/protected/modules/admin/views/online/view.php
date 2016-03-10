<?php
$this->breadcrumbs=array(
	'Online Cleaner Counts'=>array('index'),
	$model->id,
);

$this->menu=array(
	array('label'=>'列表 OnlineCleanerCount','url'=>array('index')),
	array('label'=>'添加 OnlineCleanerCount','url'=>array('create')),
	array('label'=>'编辑 OnlineCleanerCount','url'=>array('update','id'=>$model->id)),
);
?>

<?php $this->widget('bootstrap.widgets.TbDetailView',array(
	'data'=>$model,
	'attributes'=>array(
		'id',
		'date',
		'total',
		'add_time',
		'date_char',
	),
)); ?>
