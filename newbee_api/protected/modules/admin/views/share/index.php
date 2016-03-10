<?php
$this->breadcrumbs=array(
	'Share Texts'=>array('index'),
	'Manage',
);

$this->menu=array(
	array('label'=>'列表','url'=>array('index')),
	array('label'=>'创建','url'=>array('create')),
);

Yii::app()->clientScript->registerScript('search', "
$('.search-form form').submit(function(){
	$.fn.yiiGridView.update('share-text-grid', {
		data: $(this).serialize()
	});
	return false;
});
");
?>


<div class="search-form">
<?php $this->renderPartial('_search',array(
	'model'=>$model,
)); ?>
</div><!-- search-form -->

<?php $this->widget('bootstrap.widgets.TbGridView',array(
	'id'=>'share-text-grid',
	'dataProvider'=>$model->search(),
	'type'=>'striped bordered',
	'summaryCssClass' => 'breadcrumb',
	'columns'=>array(
		'id',
		'title',
		'content',
		'page_url',
		array('name' => 'image_path','value' => 'CHtml::image(AttachHelper::getFileUrl($data->image_path),"",array("width"=>"100px","height"=>"100px"))','type'=>'raw'),
		array(
			'class'=>'bootstrap.widgets.TbButtonColumn',
			'template'=>'{view} {update} {delete}',
			'header'=>'操作',
		)
	),
)); ?>
