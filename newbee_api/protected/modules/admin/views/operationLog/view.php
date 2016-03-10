<?php
$this->breadcrumbs=array(
	'Operation Logs'=>array('index'),
	$model->id,
);

$this->menu=array(
	array('label'=>'列表 OperationLog','url'=>array('index')),
	array('label'=>'添加 OperationLog','url'=>array('create')),
	array('label'=>'编辑 OperationLog','url'=>array('update','id'=>$model->id)),
);
?>

<?php $this->widget('bootstrap.widgets.TbDetailView',array(
	'data'=>$model,
	'attributes'=>array(
		'id',
		'user_id',
		'object_id',
		'operation',
		'after_value',
		'add_time',
	),
)); ?>
