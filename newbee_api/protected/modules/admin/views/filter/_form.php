<?php $form=$this->beginWidget('bootstrap.widgets.TbActiveForm',array(
	'id'=>'cleaner-status-form',
	'enableAjaxValidation'=>false,
)); ?>

	<p class="help-block note">带 <span class="required">*</span> 字段必须填写.</p>

	<?php echo $form->errorSummary($model); ?>

	<?php echo $form->textFieldRow($model,'id',array('class'=>'span5','maxlength'=>50,'readonly'=>'readonly')); ?>

	<?php echo $form->textFieldRow($model,'qrcode',array('class'=>'span5','maxlength'=>50,'readonly'=>'readonly')); ?>

    <div class="control-group ">
		<label class="control-label required" for="CleanerStatus_qrcode">选择滤芯 <span class="required">*</span></label>
		<div class="controls">
			<select name="filter_id">
				<?php
				foreach($life as $val)
				{
					echo sprintf('<option value="%s">%s</option>',$val['id'],implode(' ',array($val['id'],$val['name'],'寿命：'.$val['surplus_life'],' 剩余：'.$val['life'])));
				}
				?>
			</select>
		</div>
	</div>



    <div class="control-group ">
        <label class="control-label required" for="CleanerStatus_qrcode">寿命值 <span class="required">*</span></label>
        <div class="controls">
            <input class="span5" maxlength="50"  name="life_value" type="text" value="" />
        </div>
    </div>






<div class="form-actions">
		<?php $this->widget('bootstrap.widgets.TbButton', array(
			'buttonType'=>'submit',
			'type'=>'primary',
			'label'=>'保存',
		)); ?>
	</div>

<?php $this->endWidget(); ?>
