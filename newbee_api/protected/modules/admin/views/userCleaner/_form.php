<?php $form=$this->beginWidget('bootstrap.widgets.TbActiveForm',array(
	'id'=>'user-cleaner-form',
	'enableAjaxValidation'=>false,
)); ?>

	<p class="help-block note">带 <span class="required">*</span> 字段必须填写.</p>

	<?php echo $form->errorSummary($model); ?>

	<?php echo $form->textFieldRow($model,'user_id',array('class'=>'span5','maxlength'=>10)); ?>

	<?php echo $form->textFieldRow($model,'cleaner_id',array('class'=>'span5','maxlength'=>50)); ?>

	<?php echo $form->textFieldRow($model,'name',array('class'=>'span5','maxlength'=>50)); ?>

	<?php echo $form->textFieldRow($model,'point_x',array('class'=>'span5','maxlength'=>30)); ?>

	<?php echo $form->textFieldRow($model,'point_y',array('class'=>'span5','maxlength'=>30)); ?>

	<?php echo $form->textFieldRow($model,'city',array('class'=>'span5','maxlength'=>50)); ?>

	<?php echo $form->textFieldRow($model,'add_time',array('class'=>'span5','maxlength'=>10)); ?>

	<?php echo $form->textFieldRow($model,'wifi_name',array('class'=>'span5','maxlength'=>30)); ?>

	<?php echo $form->textFieldRow($model,'wifi_pwd',array('class'=>'span5','maxlength'=>50)); ?>

	<div class="form-actions">
		<?php $this->widget('bootstrap.widgets.TbButton', array(
			'buttonType'=>'submit',
			'type'=>'primary',
			'label'=>$model->isNewRecord ? '创建' : '保存',
		)); ?>
	</div>

<?php $this->endWidget(); ?>
