<?php
$this->breadcrumbs=array(
	'Users'=>array('index'),
	'Manage',
);
$this->menu=array(

    array('label'=>'添加 User','url'=>array('create')),

);
Yii::app()->clientScript->registerScript('search', "
$('.search-form form').submit(function(){
	$.fn.yiiGridView.update('user-grid', {
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
	'id'=>'user-grid',
	'dataProvider'=>$model->search(),
	'type'=>'striped bordered',
	'summaryCssClass' => 'breadcrumb',
	'columns'=>array(
		'id',
		'nickname',
		'mobile',
        'email',
		'app_version',
		array('name' => 'add_time', 'value' => 'date("Y-m-d H:i:s", $data->add_time)'),
		array(
			'class'=>'bootstrap.widgets.TbButtonColumn',
			'template'=>'{view} {update} {delete}',
			'header'=>'操作',
			'updateButtonLabel' => '修改',
            'deleteConfirmation' => '删除后不可恢复，确定要删除此用户吗？',
		)
	),
)); ?>
