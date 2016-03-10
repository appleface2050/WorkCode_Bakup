<!DOCTYPE html>
<html lang="en">
<head>
<?php $this->renderPartial('/layouts/common/header'); ?></head>
<body>
	
	<div class="navbar">
		<div class="navbar-inner">
			<div class="container-fluid">
				<a class="brand" href="<?php echo Yii::app()->baseUrl; ?>">
					<span>净化器后台管理</span>
				</a>
				
				<div class="btn-group pull-right">
					<a class="btn dropdown-toggle" data-toggle="dropdown" href="#">
					<i class="icon-user"></i><span class="hidden-phone"> <?php echo Yii::app()->user->name; ?>					</span> <span class="caret"></span>
					</a>
					<ul class="dropdown-menu">
						<li><?php echo CHtml::link('退出',array('login/logout')); ?></li>
					</ul>
				</div>
			</div>
		</div>
	</div>
	
	<div class="container-fluid">
		<div class="row-fluid">
			<div class="span2 main-menu-span">
				<div class="well nav-collapse sidebar-nav">
					<ul class="nav nav-tabs nav-stacked main-menu">
						<li class="nav-header hidden-tablet">Main</li>
						<li><a class="ajax-link" href="/admin/default/"><i class="icon icon-blue icon-flag"></i><span class="hidden-tablet">首页</span></a></li>
						<li><a class="ajax-link" href="/admin/cleaner/"><i class="icon icon-blue icon-users"></i><span class="hidden-tablet">净化器</span></a></li>
						<li><a class="ajax-link" href="/admin/cleaner/BatchClear"><i class="icon icon-blue icon-users"></i><span class="hidden-tablet">批量解除绑定</span></a></li>
						<li><a class="ajax-link" href="/admin/user"><i class="icon icon-blue icon-contacts"></i><span class="hidden-tablet">用户</span></a></li>
						<li><a class="ajax-link" href="/admin/aqi"><i class="icon icon-blue icon-contacts"></i><span class="hidden-tablet">PM25</span></a></li>
						<li><a class="ajax-link" href="/admin/active"><i class="icon icon-blue icon-contacts"></i><span class="hidden-tablet">活跃用户统计</span></a></li>
						<li><a class="ajax-link" href="/admin/online"><i class="icon icon-blue icon-contacts"></i><span class="hidden-tablet">活跃净化器统计</span></a></li>
                        <!-- <li><a class="ajax-link" href="/admin/online/online"><i class="icon icon-blue icon-contacts"></i><span class="hidden-tablet">在线净化器</span></a></li> -->
                        <li><a class="ajax-link" href="/admin/TempCleaner/index"><i class="icon icon-blue icon-contacts"></i><span class="hidden-tablet">净化器临时SN</span></a></li>
                        <li><a class="ajax-link" href="/admin/upgrade"><i class="icon icon-blue icon-contacts"></i><span class="hidden-tablet">在线升级</span></a></li>
                        <li><a class="ajax-link" href="/admin/admin"><i class="icon icon-blue icon-contacts"></i><span class="hidden-tablet">管理员管理</span></a></li>
						<li><a class="ajax-link" href="/admin/freeSn"><i class="icon icon-green icon-gift"></i><span class="hidden-tablet">添加活动滤芯</span></a></li>
						<li><a class="ajax-link" href="/admin/freeExchange"><i class="icon icon-green icon-gift"></i><span class="hidden-tablet">兑换</span></a></li>
						<li><a class="ajax-link" href="/admin/share"><i class="icon icon-green icon-gift"></i><span class="hidden-tablet">分享文本</span></a></li>
					</ul>
				</div>
			</div>

			<div id="content" class="span10">
				<div>
					<div class="breadcrumb">

					<?php
						if(empty($this->breadcrumbs))
							$this->breadcrumbs = array ('');
						$this->widget('zii.widgets.CBreadcrumbs', array(
					 		'links'=>$this->breadcrumbs,
							'homeLink'=>'<a href="/manage">首页</a>'
						)); ?>					
	

					</div>
				</div>
				
				<?php echo $content; ?>
			</div>
			
		</div>
		
	</div>
	
	<?php echo $this->renderPartial('/layouts/common/footer');?></body>
</html>
