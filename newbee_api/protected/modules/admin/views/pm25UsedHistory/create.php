<?php
$this->breadcrumbs=array(
	'Pm25 Used Histories'=>array('index'),
	'Create',
);

$this->menu=array(
	array('label'=>'列表 Pm25UsedHistory','url'=>array('index')),
);
?>

<?php echo $this->renderPartial('_form', array('model'=>$model)); ?>