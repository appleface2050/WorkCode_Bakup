<?php
$this->breadcrumbs=array(
	'列表'=>array('index'),
	$model->id,
);

$this->menu=array(
	array('label'=>'列表','url'=>array('index')),
	array('label'=>'添加','url'=>array('create')),
	array('label'=>'编辑','url'=>array('update','id'=>$model->id)),
);
?>

<?php $this->widget('bootstrap.widgets.TbDetailView',array(
	'data'=>$model,
	'attributes'=>array(
		'id',
		'qrcode',
		'remark',
		 array('name'=>'create_time','value' => date("Y-m-d H:i",$model->create_time)),
	),
)); ?>
