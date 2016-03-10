<?php
$this->breadcrumbs=array(
	'列表'=>array('index'),
	$model->id=>array('view','id'=>$model->id),
	'更新',
);

$this->menu=array(
	array('label'=>'列表','url'=>array('index')),
	array('label'=>'添加','url'=>array('create')),
	array('label'=>'详情','url'=>array('view','id'=>$model->id)),
);
?>

<?php echo $this->renderPartial('_form',array('model'=>$model)); ?>