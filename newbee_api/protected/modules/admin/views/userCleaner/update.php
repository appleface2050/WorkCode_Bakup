<?php
$this->breadcrumbs=array(
	'User Cleaners'=>array('index'),
	$model->name=>array('view','id'=>$model->id),
	'Update',
);

$this->menu=array(
	array('label'=>'列表 UserCleaner','url'=>array('index')),
	array('label'=>'添加 UserCleaner','url'=>array('create')),
	array('label'=>'详情 UserCleaner','url'=>array('view','id'=>$model->id)),
);
?>

<?php echo $this->renderPartial('_form',array('model'=>$model)); ?>