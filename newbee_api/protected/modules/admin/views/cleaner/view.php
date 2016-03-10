<?php
$this->breadcrumbs=array(
	'净化器'=>array('index'),
	$model->id,
);

?>

<?php $this->widget('bootstrap.widgets.TbDetailView',array(
	'data'=>$model,
	'attributes'=>array(
		'id',
		'qrcode',
		//'first_use_time' => ,
        array('name'=>'first_use_time','value'=>date("Y-m-d H:i:s",$model->first_use_time)),
		'city',
		'level',
		'childlock',
		'automatic',
		'operator_uid',
		'aqi',
		//'update_time',
         array('name'=>'update_time','value'=>$model->update_time?date("Y-m-d H:i:s",$model->update_time):''),
		'status',
		'point_x',
		'point_y',
		'switch',
		'voc',
        'version',
        'filter_surplus_life'
	),
)); ?>
