<?php
$this->breadcrumbs=array(
	'Admins'=>array('index'),
	$model->id=>array('view','id'=>$model->id),
	'Update',
);

$this->menu=array(
	array('label'=>'列表 Admin','url'=>array('index')),
	array('label'=>'添加 Admin','url'=>array('create')),
	array('label'=>'详情 Admin','url'=>array('view','id'=>$model->id)),
);
?>

<?php echo $this->renderPartial('_form',array('model'=>$model)); ?>