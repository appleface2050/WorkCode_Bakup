
<?php $this->beginContent('/layouts/main'); ?>
	<div class="box">
		<div data-original-title="" class="box-header well">
			<h2><?php if (!empty($this->title)) echo $this->title;?></h2>
		</div>
		<div class="box-content">
			<?php echo $content;?>		</div>
	</div>
	
<?php $this->endContent();?>