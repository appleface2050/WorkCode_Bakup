<?php
$this->breadcrumbs=array(
	'Aqi Indexes'=>array('index'),
	$model->name=>array('view','id'=>$model->city),
	'Update',
);

$this->menu=array(
	array('label'=>'列表 AqiIndex','url'=>array('index')),
	array('label'=>'添加 AqiIndex','url'=>array('create')),
	array('label'=>'详情 AqiIndex','url'=>array('view','id'=>$model->city)),
);
?>

<?php echo $this->renderPartial('_form',array('model'=>$model)); ?>