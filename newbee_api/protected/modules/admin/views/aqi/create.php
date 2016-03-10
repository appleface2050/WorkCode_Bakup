<?php
$this->breadcrumbs=array(
	'Aqi Indexes'=>array('index'),
	'Create',
);

$this->menu=array(
	array('label'=>'列表 AqiIndex','url'=>array('index')),
);
?>

<?php echo $this->renderPartial('_form', array('model'=>$model)); ?>