<?php $form=$this->beginWidget('bootstrap.widgets.TbActiveForm',array(
	'action'=>Yii::app()->createUrl($this->route),
	'method'=>'get',
)); ?>

<div class="control-group ">
    <label class="control-label" for="s_date">起始时间</label>
    <div class="controls">
        <input class="span5" maxlength="10" value="<?php echo isset($_GET['s_date'])?$_GET['s_date']:'';?>" name="s_date" id="s_date" type="text" class="Wdate inp" onclick="WdatePicker({dateFmt:'yyyy-MM-dd',readOnly:true})">
    </div>
</div>


<div class="control-group ">
    <label class="control-label" for="e_date">结束时间</label>
    <div class="controls">
        <input class="span5" maxlength="10" value="<?php echo isset($_GET['e_date'])?$_GET['e_date']:'';?>" name="e_date" id="e_date" type="text" class="Wdate inp" onclick="WdatePicker({dateFmt:'yyyy-MM-dd',readOnly:true,minDate:'#F{$dp.$D(\'s_date\')}'})">
    </div>
</div>
		<?php echo $form->textFieldRow($model,'object_id',array('class'=>'span5','maxlength'=>30)); ?>

		<div class="form-actions">
		<?php $this->widget('bootstrap.widgets.TbButton', array(
			'buttonType'=>'submit',
			'type'=>'primary',
			'label'=>'查询',
		)); ?>
	</div>

<?php $this->endWidget(); ?>
<script src="/admin/js/My97DatePicker/WdatePicker.js"></script>