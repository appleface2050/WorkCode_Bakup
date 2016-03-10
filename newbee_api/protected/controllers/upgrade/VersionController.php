<?php

/**
 * 净化器升级版本管理
 * @author zhoujianjun
 *
 */
class VersionController extends RestController
{
	
	private $auth = '#aircleanerupgrade#';
	
	
	/**
	 * (non-PHPdoc)
	 * @see CController::beforeAction()
	 */
	public function beforeAction($action)
	{
		return true;
	}
	
	/**
	 * 增加版本
	 */
	public function actionCreate()
	{
		$auth = Yii::app()->request->getParam('auth');
		if (empty($auth) || ($auth != $this->auth))
		{
			throw new CHttpException(403, '无权操作');
		}
		// 升级的版本号
		$version = trim(Yii::app()->request->getParam('version'));
		$client_id = Yii::app()->request->getParam('client_id');
		$description = trim(Yii::app()->request->getParam('description', ''));
		
		$upgrade = new UpgradeProgram();
		$time = time();
		$upgrade->attributes = array(
			'version' => $version,
			'client_id' => $client_id,
			'date_char' => date('Y-m-d', $time),
			'add_time' => $time,
			'description' => $description
		);
		if ($upgrade->save())
		{
			$this->afterCreate($upgrade);
			$this->success();
		}
		else 
		{
			//print_r($upgrade->getErrors());die;
			$this->failed('失败');
		}
	}
	
	/**
	 * 升级后
	 */
	public function afterCreate($model)
	{
		if (!empty($model->filepath))
		{
			$filepath = UpgradeProgram::model()->getAbsoluteFilepath($model->filepath);
			
			if (file_exists($filepath))
			{
				
				$type = $this->map($model->client_id);
				// 将更新程序的路径保存到redis里
				Yii::app()->redis->hSet(EKeys::getUpgradeProgramKey(), $this->map($model->client_id), $filepath);
				
				// 通过publish的方式  更改nodejs里 已加载的升级程序
				Yii::app()->publish->publishUpgrade($type);
			}
			
		}
	}
	
	/**
	 * 
	 */
	private function map($client_id)
	{
		$map = array_flip(CleanerStatus::getMap());
		return $map[$client_id];
	}
	
}
