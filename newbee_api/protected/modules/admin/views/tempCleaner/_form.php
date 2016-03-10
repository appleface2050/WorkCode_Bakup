<?php $form=$this->beginWidget('bootstrap.widgets.TbActiveForm',array(
	'id'=>'cleaner-status-form',
	'enableAjaxValidation'=>false,
)); ?>

	<p class="help-block note">带 <span class="required">*</span> 字段必须填写.</p>

	<?php echo $form->errorSummary($model); ?>

	<?php echo $form->textFieldRow($model,'id',array('class'=>'span5','maxlength'=>255)); ?>

	<?php echo $form->textFieldRow($model,'qrcode',array('class'=>'span5','maxlength'=>100)); ?>

	<?php echo $form->textFieldRow($model,'first_use_time',array('class'=>'span5','maxlength'=>10)); ?>

	<?php echo $form->textFieldRow($model,'filter_surplus_life',array('class'=>'span5','maxlength'=>50)); ?>

	<?php echo $form->textFieldRow($model,'city',array('class'=>'span5','maxlength'=>10)); ?>

	<?php echo $form->textFieldRow($model,'level',array('class'=>'span5')); ?>

	<?php echo $form->textFieldRow($model,'childlock',array('class'=>'span5')); ?>

	<?php echo $form->textFieldRow($model,'status',array('class'=>'span5')); ?>

	<?php echo $form->textFieldRow($model,'timeset',array('class'=>'span5','maxlength'=>500)); ?>

	<?php echo $form->textFieldRow($model,'automatic',array('class'=>'span5')); ?>

	<?php echo $form->textFieldRow($model,'operator_uid',array('class'=>'span5','maxlength'=>10)); ?>

	<?php echo $form->textFieldRow($model,'aqi',array('class'=>'span5','maxlength'=>10)); ?>

	<?php echo $form->textFieldRow($model,'update_time',array('class'=>'span5','maxlength'=>10)); ?>

	<?php echo $form->textFieldRow($model,'silence',array('class'=>'span5')); ?>

	<?php echo $form->textFieldRow($model,'silence_start',array('class'=>'span5','maxlength'=>5)); ?>

	<?php echo $form->textFieldRow($model,'silence_end',array('class'=>'span5','maxlength'=>5)); ?>

	<?php echo $form->textFieldRow($model,'point_x',array('class'=>'span5','maxlength'=>30)); ?>

	<?php echo $form->textFieldRow($model,'point_y',array('class'=>'span5','maxlength'=>30)); ?>

	<?php echo $form->textFieldRow($model,'switch',array('class'=>'span5')); ?>

	<?php echo $form->textFieldRow($model,'type',array('class'=>'span5')); ?>

	<?php echo $form->textFieldRow($model,'voc',array('class'=>'span5')); ?>

	<?php echo $form->textFieldRow($model,'version',array('class'=>'span5','maxlength'=>5)); ?>

	<div class="form-actions">
		<?php $this->widget('bootstrap.widgets.TbButton', array(
			'buttonType'=>'submit',
			'type'=>'primary',
			'label'=>$model->isNewRecord ? '创建' : '保存',
		)); ?>
	</div>

<?php $this->endWidget(); ?>
