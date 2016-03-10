<?php
$this->breadcrumbs=array(
	'活跃用户'=>array('index'),
	'活跃用户',
);

$this->menu=array(
	array('label'=>'List ActiveUserCount','url'=>array('index')),
	array('label'=>'Create ActiveUserCount','url'=>array('create')),
);

Yii::app()->clientScript->registerScript('search', "
$('.search-form form').submit(function(){
	$.fn.yiiGridView.update('active-user-count-grid', {
		data: $(this).serialize()
	});
	return false;
});
");
?>


<!-- <div class="search-form"> -->

<!-- </div> -->
<!-- search-form -->

<?php $this->widget('bootstrap.widgets.TbGridView',array(
	'id'=>'active-user-count-grid',
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
			'header'=>'查看活跃用户',
			'label' => '查看',
			'urlExpression'=>'Yii::app()->controller->createUrl("/admin/active/date", array("date" => $data->date))',
			'linkHtmlOptions'=>array('title'=>'查看')
		),
	),
)); ?>
