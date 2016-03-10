<?php
$this->breadcrumbs=array(
	'Goods'=>array('index'),
	'Manage',
);

$this->menu=array(
	array('label'=>'列表','url'=>array('index')),
	array('label'=>'创建商品','url'=>array('create')),
);

Yii::app()->clientScript->registerScript('search', "
$('.search-form form').submit(function(){
	$.fn.yiiGridView.update('goods-grid', {
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
	'id'=>'goods-grid',
	'dataProvider'=>$model->search(),
	'type'=>'striped bordered',
	'summaryCssClass' => 'breadcrumb',
	'columns'=>array(
		'id',
		'name',
		array('name' => 'cover','type' => 'raw','value' => 'CHtml::image(AttachHelper::getFileUrl($data->cover),"",array("width"=>"100px"))'),
		'cost_credit',
        'exchange_index',
		'quantity',
		array('name' => 'status','value'=>'Goods::getStatus($data->status)'),
		'rank',
        //array('name' => 'create_time', 'value' => 'date("Y-m-d H:i:s", $data->create_time)'),
		array(
			'class'=>'bootstrap.widgets.TbButtonColumn',
			'template'=>'{view} {update} {delete}',
			'header'=>'操作',
		)
	),
)); ?>
