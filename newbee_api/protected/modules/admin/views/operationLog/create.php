<?php
$this->breadcrumbs=array(
	'Operation Logs'=>array('index'),
	'Create',
);

$this->menu=array(
	array('label'=>'列表 OperationLog','url'=>array('index')),
);
?>

<?php echo $this->renderPartial('_form', array('model'=>$model)); ?>