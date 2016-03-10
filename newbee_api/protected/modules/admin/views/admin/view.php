<?php
$this->breadcrumbs=array(
	'Admins'=>array('index'),
	$model->id,
);

$this->menu=array(
	array('label'=>'列表 Admin','url'=>array('index')),
	array('label'=>'添加 Admin','url'=>array('create')),
	array('label'=>'编辑 Admin','url'=>array('update','id'=>$model->id)),
);
?>

<?php $this->widget('bootstrap.widgets.TbDetailView',array(
	'data'=>$model,
	'attributes'=>array(
		'id',
		'username',
		'superuser',
		'status',
        array('name' => 'create_at', 'value' => date("Y-m-d H:i:s", $model->create_at)),
	),
)); ?>
