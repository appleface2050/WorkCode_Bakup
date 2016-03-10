<?php $form=$this->beginWidget('bootstrap.widgets.TbActiveForm',array(
	'id'=>'admin-form',
	'enableAjaxValidation'=>false,
)); ?>

	<p class="help-block note">带 <span class="required">*</span> 字段必须填写.</p>

	<?php echo $form->errorSummary($model); ?>

	<?php echo $form->textFieldRow($model,'username',array('class'=>'span5','maxlength'=>20)); ?>

	<?php $model->password = '';echo $form->passwordFieldRow($model,'password',array('class'=>'span5','maxlength'=>128)); ?>

    <?php //echo $form->dropDownListRow($model,'superuser',array('0'=>'否',1=>'是')); ?>

	<?php //echo $form->textFieldRow($model,'status',array('class'=>'span5')); ?>

	<div class="form-actions">
		<?php $this->widget('bootstrap.widgets.TbButton', array(
			'buttonType'=>'submit',
			'type'=>'primary',
			'label'=>$model->isNewRecord ? '创建' : '保存',
		)); ?>
	</div>

<?php $this->endWidget(); ?>
