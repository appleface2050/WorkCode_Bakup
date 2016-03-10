<?php $form=$this->beginWidget('bootstrap.widgets.TbActiveForm',array(
	'id'=>'filter-exchange-form',
	'enableAjaxValidation'=>false,
)); ?>

	<p class="help-block note">带 <span class="required">*</span> 字段必须填写.</p>

	<?php echo $form->errorSummary($model); ?>

	<?php echo $form->textAreaRow($model,'remark',array('class'=>'span5','maxlength'=>300)); ?>

    <?php echo $form->dropDownListRow($model,'status',FilterExchange::getStatus(), array('class'=>'span5')); ?>

	<?php echo $form->textFieldRow($model,'shipping_info',array('class'=>'span5','maxlength'=>300)); ?>

	<div class="form-actions">
		<?php $this->widget('bootstrap.widgets.TbButton', array(
			'buttonType'=>'submit',
			'type'=>'primary',
			'label'=>$model->isNewRecord ? '创建' : '保存',
		)); ?>
	</div>

<?php $this->endWidget(); ?>
