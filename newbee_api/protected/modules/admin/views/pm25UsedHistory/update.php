<?php
$this->breadcrumbs=array(
	'Pm25 Used Histories'=>array('index'),
	$model->id=>array('view','id'=>$model->id),
	'Update',
);

$this->menu=array(
	array('label'=>'列表 Pm25UsedHistory','url'=>array('index')),
	array('label'=>'添加 Pm25UsedHistory','url'=>array('create')),
	array('label'=>'详情 Pm25UsedHistory','url'=>array('view','id'=>$model->id)),
);
?>

<?php echo $this->renderPartial('_form',array('model'=>$model)); ?>