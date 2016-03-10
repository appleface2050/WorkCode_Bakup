<?php
$this->breadcrumbs=array(
	//'User Cleaners'=>array('index'),
	'净化器关联的用户',
    $model->cleaner_id
);

/*$this->menu=array(
	array('label'=>'List UserCleaner','url'=>array('index')),
	array('label'=>'Create UserCleaner','url'=>array('create')),
);*/

Yii::app()->clientScript->registerScript('search', "
$('.search-form form').submit(function(){
	$.fn.yiiGridView.update('user-cleaner-grid', {
		data: $(this).serialize()
	});
	return false;
});
");
?>


<!-- search-form -->

<?php $this->widget('bootstrap.widgets.TbGridView',array(
	'id'=>'user-cleaner-grid',
	'dataProvider'=>$model->search(),
	'type'=>'striped bordered',
	'summaryCssClass' => 'breadcrumb',
	'columns'=>array(
		'id',
        array('name' => 'user_id', 'value' => '$data->user["nickname"]'),
        array('name' => '手机号码', 'value' => '$data->user["mobile"]'),
		'name',
        array('name' => 'add_time', 'value' => 'date("Y-m-d H:i:s", $data->add_time)'),
		array(
			'class'=>'bootstrap.widgets.TbButtonColumn',
			'template'=>'{delete}',
			'header'=>'操作',
            'deleteButtonLabel' => '取消绑定',
            'deleteConfirmation' => '确定要解该用户与净化器的绑定吗？',
		)
	),
)); ?>
