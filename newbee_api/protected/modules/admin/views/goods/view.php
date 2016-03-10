<?php
$this->breadcrumbs=array(
	'Goods'=>array('index'),
	$model->name,
);

$this->menu=array(
	array('label'=>'列表 Goods','url'=>array('index')),
	array('label'=>'添加 Goods','url'=>array('create')),
	array('label'=>'编辑 Goods','url'=>array('update','id'=>$model->id)),
);
?>

<?php $this->widget('bootstrap.widgets.TbDetailView',array(
	'data'=>$model,
	'attributes'=>array(
		'id',
		'name',
		//'cover',
		'cost_credit',
		'market_price',
		'quantity',
		'exchange_index',
		'unlock_days',
		'rank',
		array('name' => 'status','value'=>Goods::getStatus($model->status)),
		array('name' => 'create_time', 'value' => date("Y-m-d H:i:s", $model->create_time)),
		array('name' => 'update_time', 'value' => date("Y-m-d H:i:s", $model->update_time)),
	),
)); ?>
