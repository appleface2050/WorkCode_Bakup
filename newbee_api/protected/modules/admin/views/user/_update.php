<?php $form=$this->beginWidget('bootstrap.widgets.TbActiveForm',array(
	'id'=>'user-form',
	'enableAjaxValidation'=>false,
)); ?>


	<p class="help-block note">给 <span class="required"><?php echo $model->mobile;?></span> 重置密码.</p>
	
	<?php echo $form->errorSummary($model); ?>
	
	<?php echo $form->textFieldRow($model,'password',array('class'=>'span5','maxlength'=>32, 'value' => '')); ?>

    <?php echo $form->dropDownListRow($model,'is_admin',array('0'=>'否',1=>'是')); ?>


	<div class="form-actions">
		<?php $this->widget('bootstrap.widgets.TbButton', array(
			'buttonType'=>'submit',
			'type'=>'primary',
			'label'=>$model->isNewRecord ? '创建' : '保存',
		)); ?>
	</div>

<?php $this->endWidget(); ?>
