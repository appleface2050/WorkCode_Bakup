<?php
$this->breadcrumbs=array(
	'Active User Counts'=>array('index'),
	'Create',
);

$this->menu=array(
	array('label'=>'列表 ActiveUserCount','url'=>array('index')),
);
?>

<?php echo $this->renderPartial('_form', array('model'=>$model)); ?>