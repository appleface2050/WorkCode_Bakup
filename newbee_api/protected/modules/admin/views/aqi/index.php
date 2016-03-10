<?php
$this->breadcrumbs=array(
	'Aqi Indexes'=>array('index'),
	'Manage',
);

$this->menu=array(
	array('label'=>'List AqiIndex','url'=>array('index')),
	array('label'=>'Create AqiIndex','url'=>array('create')),
);

Yii::app()->clientScript->registerScript('search', "
$('.search-form form').submit(function(){
	$.fn.yiiGridView.update('aqi-index-grid', {
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
	'id'=>'aqi-index-grid',
	'dataProvider'=>$model->search(),
	'type'=>'striped bordered',
	'summaryCssClass' => 'breadcrumb',
	'columns'=>array(
		'city',
		'aqi',
		array('name' => 'update_time', 'value' => 'date("Y-m-d H:i:s",$data->update_time)'),
		'time_point',
		array(
			'class'=>'bootstrap.widgets.TbButtonColumn',
			'template'=>'{view} {update}',
			'header'=>'操作',
		)
	),
)); ?>
