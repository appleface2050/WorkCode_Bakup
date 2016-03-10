<?php $form=$this->beginWidget('bootstrap.widgets.TbActiveForm',array(
	'id'=>'aqi-index-form',
	'enableAjaxValidation'=>false,
)); ?>

	<p class="help-block note">带 <span class="required">*</span> 字段必须填写.</p>

	<?php echo $form->errorSummary($model); ?>

	<?php echo $form->textFieldRow($model,'city',array('class'=>'span5','maxlength'=>20)); ?>

	<?php echo $form->textFieldRow($model,'name',array('class'=>'span5','maxlength'=>20)); ?>

	<?php echo $form->textFieldRow($model,'aqi',array('class'=>'span5')); ?>

	<?php echo $form->textFieldRow($model,'update_time',array('class'=>'span5','maxlength'=>10)); ?>

	<?php echo $form->textFieldRow($model,'time_point',array('class'=>'span5','maxlength'=>20)); ?>

	<div class="form-actions">
		<?php $this->widget('bootstrap.widgets.TbButton', array(
			'buttonType'=>'submit',
			'type'=>'primary',
			'label'=>$model->isNewRecord ? '创建' : '保存',
		)); ?>
	</div>

<?php $this->endWidget(); ?>
