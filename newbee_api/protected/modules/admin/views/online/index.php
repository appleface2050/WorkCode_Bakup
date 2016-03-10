<?php
$this->breadcrumbs=array(
	'活跃净化器统计'=>array('index')
);

Yii::app()->clientScript->registerScript('search', "
$('.search-form form').submit(function(){
	$.fn.yiiGridView.update('online-cleaner-count-grid', {
		data: $(this).serialize()
	});
	return false;
});
");
?>


<?php $this->widget('bootstrap.widgets.TbGridView',array(
	'id'=>'online-cleaner-count-grid',
	'dataProvider'=>$model->search(),
	'type'=>'striped bordered',
	'summaryCssClass' => 'breadcrumb',
	'enableSorting' => false,
	'columns'=>array(
		'id',
		'date_char',
		'total',
		array('name' => 'add_time', 'value' => 'date("Y-m-d H:i:s", $data->add_time)'),
		array(
			'class'=>'CLinkColumn',
			'header'=>'查看活跃净化器',
			'label' => '查看',
			'urlExpression'=>'Yii::app()->controller->createUrl("/admin/online/date", array("date" => $data->date))',
			'linkHtmlOptions'=>array('title'=>'查看')
		),
	),
)); ?>
