<?php
$this->breadcrumbs=array(
	'Share Texts'=>array('index'),
	$model->title,
);

$this->menu=array(
	array('label'=>'列表','url'=>array('index')),
	array('label'=>'添加','url'=>array('create')),
	array('label'=>'编辑','url'=>array('update','id'=>$model->id)),
);
?>

<?php $this->widget('bootstrap.widgets.TbDetailView',array(
	'data'=>$model,
	'attributes'=>array(
		'id',
		'title',
		'content',
		'page_url',
		array('name' => 'image_path','value' => CHtml::image(AttachHelper::getFileUrl($model->image_path),"",array("width"=>"100px","height"=>"100px")),'type'=>'raw'),
		array('name'=>'create_time','value'=>date('Y-m-d H:i',$model->create_time)),
	),
)); ?>
