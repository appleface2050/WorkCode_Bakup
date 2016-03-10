<?php
$this->breadcrumbs=array(
	'Active User Counts'=>array('index'),
	$model->id,
);

$this->menu=array(
	array('label'=>'列表 ActiveUserCount','url'=>array('index')),
	array('label'=>'添加 ActiveUserCount','url'=>array('create')),
	array('label'=>'编辑 ActiveUserCount','url'=>array('update','id'=>$model->id)),
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
