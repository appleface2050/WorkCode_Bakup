<?php
$this->breadcrumbs=array(
	//'室内Pm2.5历史记录'=>array('index'),
	'室内Pm2.5历史记录',
    $model->cleaner_id
);

/*$this->menu=array(
	array('label'=>'List Pm25UsedHistory','url'=>array('index')),
	array('label'=>'Create Pm25UsedHistory','url'=>array('create')),
);*/

Yii::app()->clientScript->registerScript('search', "
$('.search-form form').submit(function(){
	$.fn.yiiGridView.update('pm25-used-history-grid', {
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
	'id'=>'pm25-used-history-grid',
	'dataProvider'=>$dataProvider,
	'type'=>'striped bordered',
	'summaryCssClass' => 'breadcrumb',
	'columns'=>array(
		'id',
		'cleaner_id',
		'value',
		//'date',
		//'add_time'
        array('name' => 'add_time', 'value' => 'date("Y-m-d H:i:s", $data->add_time)'),
	),
)); ?>
