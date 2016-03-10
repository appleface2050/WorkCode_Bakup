<?php
$this->breadcrumbs=array(
	'Cleaner Statuses'=>array('index'),
	'Create',
);

$this->menu=array(
	array('label'=>'列表 CleanerStatus','url'=>array('index')),
);
?>

<?php echo $this->renderPartial('_form', array('model'=>$model)); ?>