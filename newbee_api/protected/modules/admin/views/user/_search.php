<?php $form=$this->beginWidget('bootstrap.widgets.TbActiveForm',array(
	'action'=>Yii::app()->createUrl($this->route),
	'method'=>'get',
)); ?>

		<?php echo $form->textFieldRow($model,'nickname',array('class'=>'span5','maxlength'=>50)); ?>

		<?php echo $form->textFieldRow($model,'mobile',array('class'=>'span5','maxlength'=>11)); ?>

        <?php echo $form->textFieldRow($model,'email',array('class'=>'span5','maxlength'=>30)); ?>

		<div class="form-actions">
		<?php $this->widget('bootstrap.widgets.TbButton', array(
			'buttonType'=>'submit',
			'type'=>'primary',
			'label'=>'搜索',
		)); ?>
	</div>

<?php $this->endWidget(); ?>
