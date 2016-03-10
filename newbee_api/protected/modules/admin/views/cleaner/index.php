<?php
$this->breadcrumbs=array(
	'净化器'=>array('index'),
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

<?php $this->widget('bootstrap.widgets.TbGridView',array(
	'id'=>'cleaner-status-grid',
	'dataProvider'=>$model->search(),
	'type'=>'striped bordered',
	'summaryCssClass' => 'breadcrumb',
	'columns'=>array(
		'id',
		'qrcode',
		'level',
		'childlock',
		'automatic',
		'aqi',
		'switch',
		'version',
        'city',
		array(
			'class'=>'bootstrap.widgets.TbButtonColumn',
			'template'=>'{view}{update}{delete}{view_pm25}{operation_log}{user}{filter}',
            'buttons' => array(
                            'view_pm25'=>array(
                                    'url'=>'Yii::app()->controller->createUrl("/admin/Pm25UsedHistory/index",array("Pm25UsedHistory[cleaner_id]"=>$data->primaryKey))',
                                    'label'=>' 【历史pm2.5】',
                                    'options'=> array('title'=>'查看历史pm2.5')
                            ),
                            'operation_log'=>array(
                                    'url'=>'Yii::app()->controller->createUrl("/admin/operationLog/index",array("OperationLog[object_id]"=>$data->primaryKey))',
                                    'label'=>'【日志】',
                                    'options'=> array('title'=>'查看操作日志')
                            ),
                            'user'=>array(
                                'url'=>'Yii::app()->controller->createUrl("/admin/userCleaner/index",array("UserCleaner[cleaner_id]"=>$data->primaryKey))',
                                'label'=>'【关联用户】',
                                'options'=> array('title'=>'查看绑定该净化器的用户')
                            ),
                            'delete'=>array(
                                'url'=>'Yii::app()->controller->createUrl("clearBind",array("id"=>$data->primaryKey))',
                            ),
							'filter'=>array(
								'url'=>'Yii::app()->controller->createUrl("/admin/filter/update",array("id"=>$data->primaryKey))',
								'label'=>'【设置滤芯】',
								'options'=> array('title'=>'设置滤芯寿命')
							)
                ),
			'header'=>'操作',
            'updateButtonLabel'=>'修改城市',
            'deleteButtonLabel'=>'清除绑定',
            'deleteConfirmation' => "此功能会执行以下操作：\r\n(1)删除净化器信息\r\n(2)删除绑定该净化器的所有用户\r\n确定要进行此操作吗？",
		)
	),
)); ?>
