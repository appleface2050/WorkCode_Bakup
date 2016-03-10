<?php
$this->breadcrumbs=array(
	'Filter Exchanges'=>array('index'),
	'Create',
);

$this->menu=array(
	array('label'=>'列表 FilterExchange','url'=>array('index')),
);
?>

<?php echo $this->renderPartial('_form', array('model'=>$model)); ?>