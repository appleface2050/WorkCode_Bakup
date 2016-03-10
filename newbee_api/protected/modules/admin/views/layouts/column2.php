<?php $this->beginContent('/layouts/main'); ?><div class="row-fluid sortable ui-sortable">
	<div class="box span9">
		<div data-original-title="" class="box-header well">
			<h2><?php if (!empty($this->title)) echo $this->title;?></h2>
		</div>

		<div class="box-content">
			<?php echo $content;?>		</div>
	</div>
	<div class="box span3">
		<div data-original-title="" class="box-header well">
			<h3>操作</h3>
		</div>
		<div class="box-content">

		<?php
			$this->beginWidget('zii.widgets.CPortlet', array(
					//'title'=>'Operations',
			));
			$this->widget('zii.widgets.CMenu', array(
					'items'=>$this->menu,
					'htmlOptions'=>array('class'=>'operations'),
			));
			$this->endWidget();
		?>
			
		</div>

	</div>
</div>
<?php $this->endContent();?>