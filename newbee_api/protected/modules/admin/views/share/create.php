<?php
$this->breadcrumbs=array(
	'Share Texts'=>array('index')
);

$this->menu=array(
	array('label'=>'列表','url'=>array('index')),
);
?>

<?php echo $this->renderPartial('_form', array('model'=>$model)); ?>