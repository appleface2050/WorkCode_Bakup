<?php

/**
 * 净化器控制,参数变更后,更新redis中该净化器的信息,即redis中一直cache着最新的净化器的参数信息
 * @author zhoujianjun
 * @tips
 * 自动净化控制的包括： 档位 | 自动净化时间设置
 * 如果手动更改了档位, 则默认关闭自动净化, 此时的 自动净化时间 是否还有效那？
 * 增加了关闭及开启净化器的功能？
 *
 */
class ControlController extends RestController
{
	
	/**
	 * 发布类型 1参数变更  2开关控制
	 * @var unknown
	 */
	const PUBLISH_TYPE_PARAMETER = 1;
	const PUBLISH_TYPE_SWITCH    = 2;
	
	
	/**
	 * (non-PHPdoc)控制之前,检测净化器是否联网
	 * @see RestController::beforeAction()
	 */
	public function beforeAction($action)
	{
		parent::beforeAction($action);
		$actionId = $action->getId();return true;
		if (!in_array($actionId, array('silenceOn', 'silenceOff', 'silenceSet')))
		{
			//非全局的静音控制操作,检测id是否在线
			$id = Yii::app()->request->getParam('id');
			if (!$this->checkOnline($id))
				throw new CHttpException(400, '净化器电源断开或WIFI未连接');
			
			//是否在线升级 如果是在线升级中 则不可以控制
			if ($this->checkUpgrade($id))
				throw new CHttpException(400, '净化器在升级中,过会再操作');
		}
		return true;
	}
	
	/**
	 * 儿童锁控制开启与否控制
	 * @param string $token
	 * @param string $id 净化器id
	 * @param int $open 1 开启 0 关闭
	 */
	public function actionChildLock()
	{
		$childlock = intval(Yii::app()->request->getParam('childlock'));
		$model = $this->loadModel(Yii::app()->user->id, Yii::app()->request->getParam('id'), true);
		$childlock = $childlock == CleanerStatus::CHILDLOCK_ON ? $childlock : CleanerStatus::CHILDLOCK_OFF;
		$attributes = array('childlock'=>$childlock,'operator_uid'=>Yii::app()->user->id, 'update_time'=>time());
		$model->cleaner->saveAttributes($attributes);
		$this->log($model->cleaner_id, OperationLog::OPERATION_CHANGE_CHILDLOCK, $model->cleaner->childlock);
		$this->publish($model->cleaner, CleanerStatus::OP_CODE_CHILDLOCK);
		$this->success();
	}
	
	/**
	 * 自动净化开启与否控制
	 * @param string $token
	 * @param string $id 净化器id
	 * @param int $open 1 开启 0 关闭
	 */
	public function actionAutomatic()
	{
		$automatic = intval(Yii::app()->request->getParam('automatic'));
		$model = $this->loadModel(Yii::app()->user->id, Yii::app()->request->getParam('id'), true);
		$automatic = $automatic == CleanerStatus::AUTOMATIC_ON ? $automatic : CleanerStatus::AUTOMATIC_OFF;
		$attributes = array('automatic'=>$automatic,'operator_uid'=>Yii::app()->user->id, 'update_time'=>time());
		$model->cleaner->saveAttributes($attributes);
		$this->log($model->cleaner_id, OperationLog::OPERATION_CHANGE_AUTOMATIC, $model->cleaner->automatic);
		$this->publish($model->cleaner, CleanerStatus::OP_CODE_AUTOMATIC);
		$this->success();
	}
	
	/**
	 * 净化器档位控制(手动净化),变换档位,则自动切换到手动控制
	 * @param string $token
	 * @param string $id 净化器id
	 * @param integer $level 档位
	 */
	public function actionLevel()
	{
		$level = intval(Yii::app()->request->getParam('level'));
		if (!CleanerStatus::checkLevel($level))
			throw new CHttpException(404, '档位不存在');
		$model = $this->loadModel(Yii::app()->user->id, Yii::app()->request->getParam('id'), true);
		if ($model->cleaner->level != $level) 
		{
			// 档位发生了变更
			$attributes = array('level'=>$level,'automatic' => CleanerStatus::AUTOMATIC_OFF,'operator_uid'=>Yii::app()->user->id, 'update_time'=>time());
			$model->cleaner->saveAttributes($attributes);
			$this->log($model->cleaner_id, OperationLog::OPERATION_CHANGE_LEVEL, $model->cleaner->level);
			$this->publish($model->cleaner, CleanerStatus::OP_CODE_LEVEL);
		}
		$this->success();
	}
	
	/**
	 * 关闭净化器
	 * @param string $toke 用户token
	 * @param string $id 净化器id
	 */
	public function actionClose()
	{
		$model = $this->loadModel(Yii::app()->user->id, Yii::app()->request->getParam('id'), true);
		// 档位变为休眠 运行状态改为关闭
		$cleaner = $model->cleaner;
// 		if ($cleaner->status == CleanerStatus::STATE_CLOSE)
// 			throw new CHttpException(400, '净化器已经关闭');

		$attributes = array('switch' => CleanerStatus::SWITCH_CLOSE,'operator_uid'=>Yii::app()->user->id, 'update_time'=>time());
		$model->cleaner->saveAttributes($attributes);
		$this->log($model->cleaner_id, OperationLog::OPERATION_CLEANER_CLOSE, $model->cleaner->switch);
		$this->publish($model->cleaner, CleanerStatus::OP_CODE_ONOFF);
		$this->success();
	}
	
	/**
	 * 打开净化器
	 * @param string $toke 用户token
	 * @param string $id 净化器id
	 */
	public function actionOpen()
	{
		$model = $this->loadModel(Yii::app()->user->id, Yii::app()->request->getParam('id'), true);
		// 档位变为静音 运行状态改为正常运行
		$attributes = array('switch' => CleanerStatus::SWITCH_OPEN,'operator_uid'=>Yii::app()->user->id, 'update_time'=>time());
		$model->cleaner->saveAttributes($attributes);
		$this->log($model->cleaner_id, OperationLog::OPERATION_CLEANER_OPEN, $model->cleaner->switch);
		$this->publish($model->cleaner, CleanerStatus::OP_CODE_ONOFF);
		$this->success();
	}
	
	/**
	 * 设置自动净化时间,只有一个接口,每次传递全部当前的净化器时间设置,时间戳以json格式保存
	 * @param string token 用户token
	 * @param string id 净化器id
	 * @param string timeSet,json对象,包含的key为 day start_time end_time open
	 * @param string day 哪一天或者哪几天,多个天以","号分割
	 * @param string start_time 开始时间
	 * @param string end_time 结束时间
	 * @param int open 是否开启 1开启 0关闭
	 */
	public function actionTimeSet()
	{
		$model = $this->loadModel(Yii::app()->user->id, Yii::app()->request->getParam('id'), true);
		$timeSet = Yii::app()->request->getParam('timeSet');
		$attributes = array('timeset' => $timeSet,'operator_uid'=>Yii::app()->user->id, 'update_time'=>time());
		$model->cleaner->saveAttributes($attributes);
		$this->log($model->cleaner_id, OperationLog::OPERATION_CHANGE_TIMESET, $model->cleaner->timeset);
		$this->publish($model->cleaner, CleanerStatus::OP_CODE_TIMESET);
		$this->success();
	}
	
	
	/**
	 * 开启静音时间(多个净化器的静音时间 都变更)
	 * @param string token
	 * @param string id  净化器id
	 */
	public function actionSilenceOn()
	{
// 		$models = $this->loadModel2(Yii::app()->user->id);
// 		$attributes = array('silence' => CleanerStatus::SILENCE_ON,'operator_uid'=>Yii::app()->user->id, 'update_time'=>time());
// 		foreach ($models as $model) 
// 		{
// 			$model->cleaner->saveAttributes($attributes);
// 			$this->log($model->cleaner_id, OperationLog::OPERATION_CHANGE_SILENCE, $model->cleaner->silence);
// 			$this->publish($model->cleaner, CleanerStatus::OP_CODE_TIMESET);
// 		}
		$this->success();
	}
	
	/**
	 * 关闭静音时间(多个净化器的静音时间 都变更)
	 * @param string token
	 * @param string id  净化器id
	 */
	public function actionSilenceOff()
	{
// 		$models = $this->loadModel2(Yii::app()->user->id);
// 		$attributes = array('silence' => CleanerStatus::SILENCE_OFF,'operator_uid'=>Yii::app()->user->id, 'update_time'=>time());
// 		foreach ($models as $model)
// 		{
// 			$model->cleaner->saveAttributes($attributes);
// 			$this->log($model->cleaner_id, OperationLog::OPERATION_CHANGE_SILENCE, $model->cleaner->silence);
// 			$this->publish($model->cleaner, CleanerStatus::OP_CODE_TIMESET);
// 		}
		$this->success();
	}
	
	/**
	 * 设置静音起止时间(多个净化器的静音时间 都变更)
	 * @param string token
	 * @param string id  净化器id
	 * @param string silence_start 静音起止时间
	 * @param string silence_end 
	 */
	public function actionSilenceSet()
	{
		$models = $this->loadModel2(Yii::app()->user->id);
		$silence_start = Yii::app()->request->getParam('start_time');
		$silence_end = Yii::app()->request->getParam('end_time');
		if (!Util::checkTimePoint($silence_start) || !Util::checkTimePoint($silence_end))
			throw new CHttpException(400, '静音时间格式错误');
		$attributes = array('silence' => CleanerStatus::SILENCE_ON,'silence_start' => $silence_start, 'silence_end' => $silence_end, 'operator_uid'=>Yii::app()->user->id, 'update_time'=>time());
		foreach ($models as $model)
		{
			$model->cleaner->saveAttributes($attributes);
			$this->log($model->cleaner_id, OperationLog::OPERATION_CHANGE_SILENCE, $model->cleaner->silence_start . '-' . $model->cleaner->silence_end);
			//$this->publish($model->cleaner, CleanerStatus::OP_CODE_TIMESET);
		}
		$this->success();
	}
	
	/**
	 * 滤芯寿命重置,重置后返回默认的剩余寿命
	 */
	public function actionReset()
	{
		$model = $this->loadModel(Yii::app()->user->id, Yii::app()->request->getParam('id'), true);
		$filter_id = intval(Yii::app()->request->getParam('filter_id'));
		$cleaner = $model->cleaner;
		$filer = json_decode($cleaner->filter_surplus_life, true);
		$default_life = CleanerStatus::model()->getDefaultLife($cleaner->type, $filter_id,$cleaner->version);
		if(!$default_life)
			throw new CHttpException(404, '无效的滤芯');
		$filer[$filter_id] = $default_life;
		$attributes = array('filter_surplus_life' => json_encode($filer),'operator_uid'=>Yii::app()->user->id, 'update_time'=>time());
		$cleaner->saveAttributes($attributes);
		$this->log($model->cleaner_id, OperationLog::OPERATION_FILTER_RESET, $cleaner->filter_surplus_life);
		$this->publish($model->cleaner, CleanerStatus::OP_CODE_RESET, array('filter_id' => $filter_id));
		//重置记录
		CleanerFilterChange::model()->reset($cleaner->id,$filter_id);
		$this->success(array('default_life' => $default_life));
	}
	
	/**
	 * 净化器在线升级
	 */
	public function actionUpgrade()
	{
		$model = $this->loadModel(Yii::app()->user->id, Yii::app()->request->getParam('id'), true);
		$this->log($model->cleaner_id, OperationLog::OPERATION_CLEANER_UPGRADE, '');
		$this->publish($model->cleaner, CleanerStatus::OP_CODE_UPGRADE);
		$this->success();
	}
	
	/**
	 * 获取用户的某个净化器
	 * @param unknown $cleaner_id
	 * @param unknown $user_id
	 * @param $withCleaner 是否获取净化器的信息,默认不获取
	 */
	public function loadModel($user_id, $cleaner_id, $withCleaner=false)
	{
		if ($withCleaner)
			$model = UserCleaner::model()->with('cleaner')->findByAttributes(array('cleaner_id' => $cleaner_id, 'user_id'=>$user_id));
		else
			$model = UserCleaner::model()->findByAttributes(array('cleaner_id' => $cleaner_id, 'user_id'=>$user_id));

		if (empty($model))
			throw new CHttpException(404, '净化器不存在');
		if ($withCleaner && empty($model->cleaner))
			throw new CHttpException(404, '您管理的净化器不存在');
		if ($model->is_verify != UserCleaner::IS_VERIFY_YES)
			throw new CHttpException(400, '无操作权限');
		return $model;
	}
	
	/**
	 * 获取用户对应的多个净化器
	 * @see ControlController::与静音相关的action
	 * @return array()
	 */
	public function loadModel2($user_id)
	{
		$data = array();
		$model = UserCleaner::model()->with('cleaner')->findAllByAttributes(array('user_id'=>$user_id));
		if (!empty($model))
		{
			foreach ($model as $value)
			{
				if (!empty($value->cleaner))
					$data[] = $value;
			}
		}
		if (empty($data))
			throw new CHttpException(404, '您管理的净化器不存在');
		return $data;
	}
	
	/**
	 * 记录操作日志
	 * @param unknown $operation
	 * @param unknown $after_value
	 */
	private function log($object_id, $operation, $after_value)
	{
// 		$log = new OperationLog();
// 		$log->attributes = array(
// 			'user_id' => Yii::app()->user->id,
// 			'object_id' => $object_id,
// 			'operation' => $operation,
// 			'after_value' => $after_value
// 		);
// 		$log->save();
	}
	
	/**
	 * 变更操作后,publis一条参数变更的消息
	 * @param Object $cleaner  净化器对象 CleanerStatus mode
	 * @param int $type 类型标识 1 参数变更 2 
	 */
	private function publish($cleaner, $op_code=CleanerStatus::OP_CODE_AUTOMATIC, $extra=array())
	{
		$key = EKeys::getCleanerCacheKey($cleaner->id);
		$message = $cleaner->id . '-' . $op_code;

		switch ($op_code) {
			case CleanerStatus::OP_CODE_ONOFF:
				// 开启或者关闭
				Yii::app()->redis->hSet($key, 'switch', intval($cleaner->switch));
				break;
			case CleanerStatus::OP_CODE_CHILDLOCK:
				// 童锁
				Yii::app()->redis->hSet($key, 'childlock', intval($cleaner->childlock));
				break;
			case CleanerStatus::OP_CODE_AUTOMATIC:
				// 自动净化
				Yii::app()->redis->hSet($key, 'automatic', intval($cleaner->automatic));
				break;
			case CleanerStatus::OP_CODE_LEVEL:
				// 变更档位
				Yii::app()->redis->hMset($key, array('level' => intval($cleaner->level), 'automatic' => CleanerStatus::AUTOMATIC_OFF));
				break;
			case CleanerStatus::OP_CODE_TIMESET:
				// 自动净化时间变更
				$timeset = CleanerStatus::model()->getFormatTimeset($cleaner);
				Yii::app()->redis->hSet($key, 'timeset', join('#', $timeset));
				 break;
			case CleanerStatus::OP_CODE_RESET:
				// 滤芯重置
				Yii::app()->redis->hSet($key, 'reset', intval($extra['filter_id']));
				break;
			case CleanerStatus::OP_CODE_UPGRADE:
				// 净化器在线升级(将请求升级的净化器id放在集合里)
				Yii::app()->redis->sAdd(EKeys::getUpgradeKey(), $cleaner->id);
				break;
			case CleanerStatus::OP_CODE_LED_ONOFF:
				// 开启或者关闭
				Yii::app()->redis->hSet($key, 'led_switch', $cleaner->led_status);
				break;
 		}
 		Yii::app()->publish->publishParameter($message);
	}

	/**
	 * 控制净化器LED灯
	 * @param string $toke 用户token
	 * @param string $id 净化器id
	 */
	public function actionLed()
	{
		$actionArr = array(
			'off'=> 0,
			'on' => 1,
		);
		$model = $this->loadModel(Yii::app()->user->id, Yii::app()->request->getParam('id'), true);
		$action = strtolower(Yii::app()->request->getParam('action'));
		if(!$action || !isset($actionArr[$action]))
			throw new CHttpException(404, '错误的指令操作');
		$cleaner = $model->cleaner;
		$attributes = array('led_status'=>$actionArr[$action], 'update_time'=>time());
		$cleaner->saveAttributes($attributes);
		$this->log($model->cleaner_id, OperationLog::OPERATION_CLEANER_LED_ONOFF,$cleaner->led_status);
		$this->publish($model->cleaner, CleanerStatus::OP_CODE_LED_ONOFF);
		$this->success();
	}
	
}