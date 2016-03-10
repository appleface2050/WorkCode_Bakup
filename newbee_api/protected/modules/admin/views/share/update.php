<?php
$this->breadcrumbs=array(
	'Share Texts'=>array('index'),
	$model->title=>array('view','id'=>$model->id),
	'Update',
);

$this->menu=array(
	array('label'=>'列表','url'=>array('index')),
	array('label'=>'添加','url'=>array('create')),
	array('label'=>'详情','url'=>array('view','id'=>$model->id)),
);
?>

<?php echo $this->renderPartial('_form',array('model'=>$model)); ?>