<?php
$this->breadcrumbs=array(
	'Pm25 Used Histories'=>array('index'),
	$model->id,
);

/*$this->menu=array(
	array('label'=>'列表 Pm25UsedHistory','url'=>array('index')),
	array('label'=>'添加 Pm25UsedHistory','url'=>array('create')),
	array('label'=>'编辑 Pm25UsedHistory','url'=>array('update','id'=>$model->id)),
);*/
?>

<?php $this->widget('bootstrap.widgets.TbDetailView',array(
	'data'=>$model,
	'attributes'=>array(
		'id',
		'cleaner_id',
		'value',
		'date',
		'add_time',
	),
)); ?>
