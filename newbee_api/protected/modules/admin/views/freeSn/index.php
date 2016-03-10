<?php
$this->breadcrumbs=array(
	'列表'=>array('index')
);

$this->menu=array(
	array('label'=>'添加SN','url'=>array('create')),
);

Yii::app()->clientScript->registerScript('search', "
$('.search-form form').submit(function(){
	$.fn.yiiGridView.update('cleaner-free-exchange-grid', {
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
	'id'=>'cleaner-free-exchange-grid',
	'dataProvider'=>$model->search(),
	'type'=>'striped bordered',
	'summaryCssClass' => 'breadcrumb',
	'columns'=>array(
		'id',
		'qrcode',
		 array('name' => 'type_id', 'value' => 'CleanerFreeExchange::getType($data->type_id)'),
		'remark',
		array('name'=>'create_time','value' => 'date("Y-m-d H:i",$data->create_time)'),
		array(
			'class'=>'bootstrap.widgets.TbButtonColumn',
			'template'=>'{view} {update} {delete}',
			'header'=>'操作',
		)
	),
)); ?>
