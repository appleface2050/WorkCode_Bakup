<?php
$this->breadcrumbs=array(
	'活跃净化器统计'=>array('/admin/online'),
	'活跃历史'
);


Yii::app()->clientScript->registerScript('search', "
$('.search-form form').submit(function(){
	$.fn.yiiGridView.update('online-cleaner-history-grid', {
		data: $(this).serialize()
	});
	return false;
});
");
?>


<?php $this->widget('bootstrap.widgets.TbGridView',array(
	'id'=>'online-cleaner-history-grid',
	'dataProvider'=>$model->search(),
	'type'=>'striped bordered',
	'summaryCssClass' => 'breadcrumb',
	'enableSorting' => false,
	'columns'=>array(
		array('name' => 'cleaner_id', 'header' => '净化器序列号', 'value' => '$data->cleaner->qrcode'),
		array('header' => '净化器ID', 'value' => '$data->cleaner->id'),
		'date_char',
		array('name' => 'add_time', 'value' => 'date("Y-m-d H:i:s", $data->add_time)'),
	),
)); ?>
