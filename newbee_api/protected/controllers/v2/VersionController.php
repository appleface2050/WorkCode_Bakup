<?php

/**
 * 版本控制
 * @author zhoujianjun
 *
 */
class VersionController extends RestController
{
	
	/**
	 * 
	 * @var unknown
	 */
	private $auth = '3f3ac015f5ef10ea2';
	
	
	/**
	 * 添加版本更新信息时,不需要登录
	 * @see RestController::beforeAction()
	 */
	public function beforeAction($action)
	{
		return true;
		/*$id = $action->getId();
		if (strtolower($id) == 'add')
		{
			return true;
		}
		else 
		{
			return parent::beforeAction($action);
		}*/
	}
	
	
	
	/**
	 * 判断最新版本
	 */
	public function actionUpdate()
	{
		$platform = Yii::app()->request->getParam('platform');
		$version  = Yii::app()->request->getParam('version');
		$default = array(
			'update' => 0,
			'update_url' => '',
			'title' => '版本更新',
			'info' => '版本更新',
			'force_update' => 0
		);
		
		$type = (($platform == Version::PLATFORM_IOS) || (stripos($platform, 'ios') !== false)) ? Version::PLATFORM_IOS : Version::PLATFORM_ANDROID;
		$data = Version::model()->getLatestOne($type, $version);
		if (empty($data))
			$this->success($default);
		else 
		{
			$data['update'] = 1;
			$this->success($data);	
		}
	}
	
	
	/**
	 * 添加新版本
	 * @param $token
	 * @param $version
	 * @param $platform
	 * @param $title
	 * @param $update_url
	 * @param $force_update
	 */
	public function actionAdd()
	{
		$auth = trim(Yii::app()->request->getParam('auth'));
		if (empty($auth) || $auth != $this->auth)
			throw new CHttpException(403, '禁止操作');
		
		$request = Yii::app()->request;
		
		$version = trim($request->getParam('version'));
		$platform = trim($request->getParam('platform'));
		$title = trim($request->getParam('title'));
		$info = trim($request->getParam('info'));
		$update_url = trim($request->getParam('update_url'));
		$force_update = intval($request->getParam('force_update'));
		
		$model = new Version();
		$model->attributes = array(
			'version' => $version,
			'platform' => $platform,
			'title' => $title,
			'info'  => $info,
			'update_url' => $update_url,
			'force_update' => $force_update
		);
		if ($model->save())
		{
			$this->success();
		}
		else
		{
			$error = array_values($model->getErrors());
			$this->failed($error[0][0]);
		}
	}
	
	
}