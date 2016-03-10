<?php
$this->breadcrumbs=array(
	'Goods'=>array('index'),
	'Create',
);

$this->menu=array(
	array('label'=>'列表 Goods','url'=>array('index')),
);
?>

<?php echo $this->renderPartial('_form', array('model'=>$model)); ?>