<?php
$this->breadcrumbs=array(
	'Filter Exchanges'=>array('index'),
	'Manage',
);

$this->menu=array(
	array('label'=>'列表','url'=>array('index')),
);

Yii::app()->clientScript->registerScript('search', "
$('.search-form form').submit(function(){
	$.fn.yiiGridView.update('filter-exchange-grid', {
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
	'id'=>'filter-exchange-grid',
	'dataProvider'=>$model->search(),
	'type'=>'striped bordered',
	'summaryCssClass' => 'breadcrumb',
	'columns'=>array(

		'filter_id',
		array('name' => 'filter_name', 'value' => 'CleanerStatus::getDefaultName($data["cleaner"]->type,$data->filter_id)'),
		array('name' => 'create_time', 'value' => 'date("Y-m-d H:i", $data->create_time)'),
		'receiver_name',
		'receiver_mobile',
		'receiver_address',
		array('name' => 'status', 'value' => 'FilterExchange::getStatus($data->status)'),
		array(
			'class'=>'bootstrap.widgets.TbButtonColumn',
			'template'=>'{view} {update}',
			'header'=>'操作',
		)
	),
)); ?>
