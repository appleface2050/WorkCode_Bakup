<?php
$this->breadcrumbs=array(
	'列表'=>array('index'),
	'添加',
);

$this->menu=array(
	array('label'=>'列表','url'=>array('index')),
);
?>

<?php echo $this->renderPartial('_form', array('model'=>$model)); ?>