<?php $form=$this->beginWidget('bootstrap.widgets.TbActiveForm',array(
	'id'=>'goods-form',
	'enableAjaxValidation'=>false,
	'htmlOptions'=>array('enctype'=>'multipart/form-data'),
)); ?>

	<p class="help-block note">带 <span class="required">*</span> 字段必须填写.</p>

	<?php echo $form->errorSummary($model); ?>

	<?php echo $form->textFieldRow($model,'name',array('class'=>'span5','maxlength'=>100)); ?>

	<?php echo $form->fileFieldRow($model,'cover',array('class'=>'span5','maxlength'=>100)); ?>

	<?php echo $form->textFieldRow($model,'cost_credit',array('class'=>'span5')); ?>

	<?php echo $form->textFieldRow($model,'market_price',array('class'=>'span5','maxlength'=>10)); ?>

	<?php echo $form->textFieldRow($model,'quantity',array('class'=>'span5')); ?>

	<?php echo $form->textFieldRow($model,'exchange_index',array('class'=>'span5')); ?>

	<?php echo $form->textFieldRow($model,'unlock_days',array('class'=>'span5')); ?>

    <?php echo $form->textFieldRow($model,'rank',array('class'=>'span5')); ?>

	<?php echo $form->dropDownListRow($model,'status',Goods::getStatus(),array('class'=>'span5')); ?>


	<div class="form-actions">
		<?php $this->widget('bootstrap.widgets.TbButton', array(
			'buttonType'=>'submit',
			'type'=>'primary',
			'label'=>$model->isNewRecord ? '创建' : '保存',
		)); ?>
	</div>

<?php $this->endWidget(); ?>
