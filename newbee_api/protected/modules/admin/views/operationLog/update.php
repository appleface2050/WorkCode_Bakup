<?php
$this->breadcrumbs=array(
	'Operation Logs'=>array('index'),
	$model->id=>array('view','id'=>$model->id),
	'Update',
);

$this->menu=array(
	array('label'=>'列表 OperationLog','url'=>array('index')),
	array('label'=>'添加 OperationLog','url'=>array('create')),
	array('label'=>'详情 OperationLog','url'=>array('view','id'=>$model->id)),
);
?>

<?php echo $this->renderPartial('_form',array('model'=>$model)); ?>