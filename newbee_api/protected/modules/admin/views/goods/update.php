<?php
$this->breadcrumbs=array(
	'Goods'=>array('index'),
	$model->name=>array('view','id'=>$model->id),
	'Update',
);

$this->menu=array(
	array('label'=>'列表 Goods','url'=>array('index')),
	array('label'=>'添加 Goods','url'=>array('create')),
	array('label'=>'详情 Goods','url'=>array('view','id'=>$model->id)),
);
?>

<?php echo $this->renderPartial('_form',array('model'=>$model)); ?>