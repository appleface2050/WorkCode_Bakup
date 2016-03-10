<?php
$this->breadcrumbs=array(
	'Cleaner Statuses'=>array('index'),
	$model->id=>array('view','id'=>$model->id),
	'Update',
);
?>

<?php echo $this->renderPartial('_form',array('model'=>$model,'life' => $life)); ?>