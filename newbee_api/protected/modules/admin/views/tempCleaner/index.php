<?php
$this->breadcrumbs=array(
	'临时净化器SN'=>array('index'),
);

Yii::app()->clientScript->registerScript('search', "
$('.search-form form').submit(function(){
	$.fn.yiiGridView.update('cleaner-status-grid', {
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

<?php
$this->widget('bootstrap.widgets.TbGridView',array(
	'id'=>'cleaner-status-grid',
	'dataProvider'=>$dataProvider,
	'type'=>'striped bordered',
	'summaryCssClass' => 'breadcrumb',
	'columns'=>array(

        array('name'=>'<input type="checkbox"/>','value'=>''),
        'id',
		'qrcode',
		array('name'=>'first_use_time','value'=>'date("Y-m-d H:i:s",$data->first_use_time)'),
		//'filter_surplus_life',
		/*
		'childlock',
		'status',
		'timeset',
		'automatic',
		'operator_uid',
		'aqi',
		'update_time',
		'silence',
		'silence_start',
		'silence_end',
		'point_x',
		'point_y',
		'switch',
		'type',
		'voc',
		'version',
		*/
		array(
			'class'=>'bootstrap.widgets.TbButtonColumn',
			'template'=>'{delete}',
            'deleteButtonLabel'=>'清除绑定',
			'header'=>'操作',
            'deleteConfirmation' => '确定要清除净化器临时SN号吗？',
		)
	),
)); ?>
<script>
    /*$(document).ready(function(){
        $('table').find("tr").each(function(index,item){
            var tdobj = $(this).find('td:eq(1)');
            var id = tdobj.html();
            if(id)
            {
                $(this).find('td:first').html('<input type="checkbox" value="'+id+'" name="ids[]"/>');
            }
        });
    })*/
</script>