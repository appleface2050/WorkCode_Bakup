<?php
$this->breadcrumbs=array(
	'Online Cleaner Counts'=>array('index'),
	$model->id=>array('view','id'=>$model->id),
	'Update',
);

$this->menu=array(
	array('label'=>'列表 OnlineCleanerCount','url'=>array('index')),
	array('label'=>'添加 OnlineCleanerCount','url'=>array('create')),
	array('label'=>'详情 OnlineCleanerCount','url'=>array('view','id'=>$model->id)),
);
?>

<?php echo $this->renderPartial('_form',array('model'=>$model)); ?>