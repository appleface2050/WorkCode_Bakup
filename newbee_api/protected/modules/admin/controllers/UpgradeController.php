<?php

Yii::import('application.modules.admin.controllers.BaseController');


/**
 * 在线升级
 * @author zhoujianjun
 *
 */
class UpgradeController extends BaseController
{
	
	/**
	 * @var string the default layout for the views. Defaults to '//layouts/column2', meaning
	 * using two-column layout. See 'protected/views/layouts/column2.php'.
	 */
	public $layout='column1';
	
	/**
	 * @return array action filters
	 */
	public function filters()
	{
		return array(
			'accessControl', // perform access control for CRUD operations
		);
	}
	
	/**
	 * Specifies the access control rules.
	 * This method is used by the 'accessControl' filter.
	 * @return array access control rules
	 */
	public function accessRules()
	{
		return array(
			array('allow', // allow authenticated user to perform 'create' and 'update' actions
				'actions'=>array('index'),
				'users'=>array('@'),
			),
			array('deny',  // deny all users
				'users'=>array('*'),
			),
		);
	}
	
	
	/**
	 * 全部在线升级的净化器
	 */
	public function actionIndex()
	{
		$key = EKeys::getUpgradeKey();
		
		$data = Yii::app()->redis->zRange($key, 0, -1, true);
		
		
		$this->render('index', array('data' => $data));
		
// 		$model=new ActiveUserCount('search');
// 		$model->unsetAttributes();  // clear any default values
// 		if(isset($_GET['ActiveUserCount']))
// 			$model->attributes=$_GET['ActiveUserCount'];
		
// 		$this->render('index',array(
// 				'model'=>$model,
// 		));
		
	}
	
	
}