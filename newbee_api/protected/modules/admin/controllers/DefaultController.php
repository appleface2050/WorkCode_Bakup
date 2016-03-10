<?php

Yii::import('application.modules.admin.controllers.BaseController');

/**
 * 登录成功后的首页, 显示一些统计信息
 * @author zhoujianjun
 *
 */
class DefaultController extends BaseController
{
	
	/**
	 * @var string the default layout for the views. Defaults to '//layouts/column2', meaning
	 * using two-column layout. See 'protected/views/layouts/column2.php'.
	 */
	public $layout='column1';
	
	
	/**
	 * 首页显示统计
	 */
	public function actionIndex()
	{
		// 当前在线净化器数量
		//$totalOnline = Yii::app()->redis->sCard(EKeys::getAllonlineKey());
		
		
		// 当前用户总数
		//$totalUser = User::model()->calculateTotal();		
		
		$this->render('index');
	}
	
	
	
}