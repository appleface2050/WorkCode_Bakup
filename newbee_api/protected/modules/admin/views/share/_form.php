<?php $form=$this->beginWidget('bootstrap.widgets.TbActiveForm',array(
	'id'=>'share-text-form',
    'htmlOptions'=>array('enctype' => 'multipart/form-data'),
	'enableAjaxValidation'=>false,
)); ?>

	<p class="help-block note">带 <span class="required">*</span> 字段必须填写.</p>

	<?php echo $form->errorSummary($model); ?>

	<?php echo $form->textFieldRow($model,'title',array('class'=>'span5','maxlength'=>100)); ?>

	<?php echo $form->textFieldRow($model,'content',array('class'=>'span5','maxlength'=>300)); ?>

    <?php echo $form->fileFieldRow($model,'image_path',array('class'=>'span5','maxlength'=>300)); ?>

	<?php echo $form->textFieldRow($model,'page_url',array('class'=>'span5')); ?>

	<div class="form-actions">
		<?php $this->widget('bootstrap.widgets.TbButton', array(
			'buttonType'=>'submit',
			'type'=>'primary',
			'label'=>$model->isNewRecord ? '创建' : '保存',
		)); ?>
	</div>

<?php $this->endWidget(); ?>
