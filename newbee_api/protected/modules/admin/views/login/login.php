<div class="row-fluid">
	<div class="span12 center login-header">
		<h2>净化器后台管理系统</h2>
	</div><!--/span-->
</div><!--/row-->
			
<div class="row-fluid">
	<div class="well span6 center login-box">
		
		<div class="alert alert-warning">
	
			<?php if(Yii::app()->user->hasFlash('loginMessage')): ?>
				<?php echo Yii::app()->user->getFlash('loginMessage'); ?>
			<?php else:?>
				请输入用户名和密码
			<?php endif;?>
		
		
		
			
		</div>
		
		<?php
			$form = $this->beginWidget('bootstrap.widgets.TbActiveForm', array(
			    'id'=>'backUserLogin',
			    'type'=>'horizontal',
		)); ?>
 
<fieldset>
		
		<?php echo $form->textFieldRow($model,'username'); ?>		
		<?php echo $form->passwordFieldRow($model, 'password'); ?>		
		<div class="control-group">
			<div class="controls">
				<?php $this->widget('bootstrap.widgets.TbButton', array('buttonType'=>'submit', 'type'=>'primary', 'size'=>'large', 'label'=>'登录'));?>			</div>
		</div>
</fieldset>

<?php $this->endWidget();?>	</div>
		
</div>

