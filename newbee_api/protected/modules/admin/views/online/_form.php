<?php $form=$this->beginWidget('bootstrap.widgets.TbActiveForm',array(
	'id'=>'online-cleaner-count-form',
	'enableAjaxValidation'=>false,
)); ?>

	<p class="help-block note">带 <span class="required">*</span> 字段必须填写.</p>

	<?php echo $form->errorSummary($model); ?>

	<?php echo $form->textFieldRow($model,'date',array('class'=>'span5','maxlength'=>10)); ?>

	<?php echo $form->textFieldRow($model,'total',array('class'=>'span5','maxlength'=>10)); ?>

	<?php echo $form->textFieldRow($model,'add_time',array('class'=>'span5','maxlength'=>10)); ?>

	<?php echo $form->textFieldRow($model,'date_char',array('class'=>'span5','maxlength'=>10)); ?>

	<div class="form-actions">
		<?php $this->widget('bootstrap.widgets.TbButton', array(
			'buttonType'=>'submit',
			'type'=>'primary',
			'label'=>$model->isNewRecord ? '创建' : '保存',
		)); ?>
	</div>

<?php $this->endWidget(); ?>
