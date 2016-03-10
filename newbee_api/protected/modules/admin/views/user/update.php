<?php
$this->breadcrumbs=array(
	'Users'=>array('index'),
	$model->id=>array('view','id'=>$model->id),
	'重置密码',
);

$this->menu=array(
	array('label'=>'列表 User','url'=>array('index')),
	array('label'=>'添加 User','url'=>array('create')),
	array('label'=>'详情 User','url'=>array('view','id'=>$model->id)),
);
?>

<?php echo $this->renderPartial('_update',array('model'=>$model)); ?>