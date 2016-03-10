<?php
$this->breadcrumbs=array(
    '净化器'=>array('index'),
);

$form=$this->beginWidget('bootstrap.widgets.TbActiveForm',array(
	'id'=>'cleaner-status-form',
	'enableAjaxValidation'=>false,
    'action' => $this->createUrl('BatchClear'),
)); ?>
    <?php
    if($error)
    {
    ?>
        <div class="alert alert-block alert-error"><p>消息提示:</p>
            <ul>
                <li><?php echo $error;?></li>
            </ul>
        </div>
    <?php
    }elseif($success)
    {
    ?>
        <div class="alert alert-block alert-success"><p><?php echo $success;?></p></div>
    <?php
    }
    ?>


    <div class="control-group">
        <label class="control-label" for="qrcodes">请输入净化器SN<br>（一行一个SN）</label>
        <div class="controls">
            <textarea class="span5" name="qrcodes" id="qrcodes" rows="20"></textarea>
        </div>
    </div>


    <div class="form-actions">
		<?php $this->widget('bootstrap.widgets.TbButton', array(
			'buttonType'=>'submit',
			'type'=>'primary',
			'label'=> '清除',
            'htmlOptions'=> array('onClick'=>"if(confirm('确定要进行此操作？')) return true;else return false;")
		)); ?>
	</div>

<?php $this->endWidget(); ?>
