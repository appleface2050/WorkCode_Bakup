<?php
$this->breadcrumbs=array(
	'Users'=>array('index'),
	$model->id,
);

$this->menu=array(
	array('label'=>'列表 User','url'=>array('index')),
	array('label'=>'添加 User','url'=>array('create')),
	array('label'=>'编辑 User','url'=>array('update','id'=>$model->id)),
);
?>

<?php $this->widget('bootstrap.widgets.TbDetailView',array(
	'data'=>$model,
	'attributes'=>array(
		'id',
		'nickname',
		'mobile',
        'email',
		'app_version',
		array('name' => 'add_time', 'value' => date("Y-m-d H:i:s", $model->add_time))
		//'add_time',
	),
)); ?>
