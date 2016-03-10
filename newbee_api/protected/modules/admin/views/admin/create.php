<?php
$this->breadcrumbs=array(
	'Admins'=>array('index'),
	'Create',
);

$this->menu=array(
	array('label'=>'列表 Admin','url'=>array('index')),
);
?>

<?php echo $this->renderPartial('_form', array('model'=>$model)); ?>