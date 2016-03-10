<?php
$this->breadcrumbs=array(
	'Filter Exchanges'=>array('index'),
	$model->id,
);

$this->menu=array(
	array('label'=>'列表','url'=>array('index')),
);
?>

<?php $this->widget('bootstrap.widgets.TbDetailView',array(
	'data'=>$model,
	'attributes'=>array(
		'id',
		'user_id',
		'cleaner_id',
		'filter_id',
		'filter_name',
		array('name' => 'create_time', 'value' => date("Y-m-d H:i", $model->create_time)),
		'receiver_name',
		'receiver_mobile',
		'receiver_address',
		 array('name' => 'status', 'value' => FilterExchange::getStatus($model->status)),
		'shipping_info',
		'remark',
	),
)); ?>
