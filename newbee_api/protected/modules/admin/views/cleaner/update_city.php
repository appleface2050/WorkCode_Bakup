<?php
$this->breadcrumbs=array(
    '净化器'=>array('index'),
    $model->id=>array('view','id'=>$model->id),
    '更新所在城市',
);

$form=$this->beginWidget('bootstrap.widgets.TbActiveForm',array(
	'id'=>'cleaner-status-form',
	'enableAjaxValidation'=>false,
    'action' => $this->createUrl('updateCity',array('id'=>$model->id)),
)); ?>

	<p class="help-block note">带 <span class="required">*</span> 字段必须填写.</p>

	<?php echo $form->errorSummary($model); ?>



	<?php
    //echo $form->textFieldRow($model,'city',array('class'=>'span5','maxlength'=>10));

    $this->widget('CAutoComplete', array(
        'value' =>$model->city,
        'name'=>'CleanerStatus[city]',
        'url'=> $this->createUrl('searchCity'),
        // additional javascript options for the autocomplete plugin
        'options'=>array(
            'minLength'=>'1',
        ),
        'htmlOptions'=>array(
            'style'=>'height:20px;',
            'placeholder'=>'请输入城市'
        ),
    ));



    ?>


	<div class="form-actions">
		<?php $this->widget('bootstrap.widgets.TbButton', array(
			'buttonType'=>'submit',
			'type'=>'primary',
			'label'=>$model->isNewRecord ? '创建' : '保存',
		)); ?>
	</div>

<?php $this->endWidget(); ?>
