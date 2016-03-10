<?php
$this->breadcrumbs=array(
	//'Operation Logs'=>array('index'),
	'净化器操作日志',
);

/*$this->menu=array(
	array('label'=>'List OperationLog','url'=>array('index')),
	array('label'=>'Create OperationLog','url'=>array('create')),
);*/

Yii::app()->clientScript->registerScript('search', "
$('.search-form form').submit(function(){
	$.fn.yiiGridView.update('operation-log-grid', {
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
	'id'=>'operation-log-grid',
	'dataProvider'=>$dataProvider,
	'type'=>'striped bordered',
	'summaryCssClass' => 'breadcrumb',
	'columns'=>array(
		'id',
		//'user_id',
		'object_id',
        array('name' => 'operation', 'value' => 'OperationLog::getOperationName($data->operation)'),
		'after_value',
        array('name' => 'add_time', 'value' => 'date("Y-m-d H:i:s", $data->add_time)'),

	),
)); ?>
