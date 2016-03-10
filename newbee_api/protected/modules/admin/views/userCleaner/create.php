<?php
$this->breadcrumbs=array(
	'User Cleaners'=>array('index'),
	'Create',
);

$this->menu=array(
	array('label'=>'列表 UserCleaner','url'=>array('index')),
);
?>

<?php echo $this->renderPartial('_form', array('model'=>$model)); ?>