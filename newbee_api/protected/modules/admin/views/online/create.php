<?php
$this->breadcrumbs=array(
	'Online Cleaner Counts'=>array('index'),
	'Create',
);

$this->menu=array(
	array('label'=>'列表 OnlineCleanerCount','url'=>array('index')),
);
?>

<?php echo $this->renderPartial('_form', array('model'=>$model)); ?>