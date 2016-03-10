<?php
$this->breadcrumbs=array(
	'Cleaner Statuses'=>array('index'),
	$model->id=>array('view','id'=>$model->id),
	'Update',
);

$this->menu=array(
	array('label'=>'列表 CleanerStatus','url'=>array('index')),
	array('label'=>'添加 CleanerStatus','url'=>array('create')),
	array('label'=>'详情 CleanerStatus','url'=>array('view','id'=>$model->id)),
);
?>

<?php echo $this->renderPartial('_form',array('model'=>$model)); ?>