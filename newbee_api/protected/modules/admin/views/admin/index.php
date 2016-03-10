<?php
$this->breadcrumbs=array(
	'Admins'=>array('index'),
	'Manage',
);

$this->menu=array(
	array('label'=>'List Admin','url'=>array('index')),
	array('label'=>'Create Admin','url'=>array('create')),
);

Yii::app()->clientScript->registerScript('search', "
$('.search-form form').submit(function(){
	$.fn.yiiGridView.update('admin-grid', {
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
	'id'=>'admin-grid',
	'dataProvider'=>$model->search(),
	'type'=>'striped bordered',
	'summaryCssClass' => 'breadcrumb',
	'columns'=>array(
		'id',
		'username',
        array('name' => 'superuser', 'value' => '$data->superuser?"是":"否";'),
		//'status',
        array('name' => 'create_at', 'value' => 'date("Y-m-d H:i:s", $data->create_at)'),
		array(
			'class'=>'bootstrap.widgets.TbButtonColumn',
			'template'=>'{update} {delete}',
			'header'=>'操作',
		)
	),
)); ?>
