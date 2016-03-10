<?php
$this->breadcrumbs=array(
	'Active User Counts'=>array('index'),
	$model->id=>array('view','id'=>$model->id),
	'Update',
);

$this->menu=array(
	array('label'=>'列表 ActiveUserCount','url'=>array('index')),
	array('label'=>'添加 ActiveUserCount','url'=>array('create')),
	array('label'=>'详情 ActiveUserCount','url'=>array('view','id'=>$model->id)),
);
?>

<?php echo $this->renderPartial('_form',array('model'=>$model)); ?>