<?php

/**
 * 
 * 申请添加净化器管理
 * @author Echo
 *
 */
class ApplyController extends RestController
{	
	
	
	/**
	 * 增加签名验证
	 * @see RestController::beforeAction()
	 */
	public function beforeAction($action)
	{
		$this->addParam();
		return parent::beforeAction($action);
	}
	
	/**
	 * 
	 * 同意添加净化器的申请
	 * Enter description here ...
	 * /apply/pass/
	 * @param token
	 * @param sign
	 * @param user_id
	 * @param cleaner_id
	 * @param apply_id 
	 */
	public function actionPass()
	{
		//$this->checkSign(array('token', 'cleaner_id', 'client_id', 'user_id', 'apply_id'));
		$request = Yii::app()->request;
		$user_id = intval($request->getParam('user_id', 0));
		$cleaner_id = trim($request->getParam('cleaner_id'));
		$apply_id = intval($request->getParam('apply_id'));
		if (empty($user_id) || empty($cleaner_id) || empty($apply_id))
			$this->failed('参数错误');
		$applyLog = UserApplyLog::model()->findByPk($apply_id);
		if (empty($applyLog))
			$this->failed('申请不存在');
		if ($applyLog->apply_uid != $user_id || $applyLog->cleaner_id != $cleaner_id)
			$this->failed('信息不符, 非法访问');
		if ($applyLog->status != UserApplyLog::APPLY_STATUS_APPLY)
			$this->failed('该申请已经审核过了');
		if (!UserCleaner::model()->checkIsCharger(Yii::app()->user->id, $cleaner_id))
			$this->failed('不是主账户没有审核权限');
		$userCleaner = UserCleaner::model()->findByAttributes(array('user_id' => $user_id, 'cleaner_id' => $cleaner_id));
		if (empty($userCleaner))
		{
			UserApplyLog::model()->deleteByPk($apply_id);
			$this->failed('没法通过申请, 控制不存在');
		}
		UserApplyLog::model()->updateByPk($apply_id, array('status' => UserApplyLog::APPLY_STATUS_PASS, 'verify_time' => time(), 'verify_uid' => Yii::app()->user->id));
		UserCleaner::model()->updateByPk($userCleaner->id, array('is_verify' => UserCleaner::IS_VERIFY_YES));
		$this->success();
	}
	
	/**
	 * 拒绝添加净化器的申请
	 * /apply/refuse
	 * @param token
	 * @param sign
	 * @param user_id
	 * @param cleaner_id
	 * @param apply_id
	 */
	public function actionRefuse()
	{
		$request = Yii::app()->request;
		$user_id = intval($request->getParam('user_id', 0));
		$cleaner_id = trim($request->getParam('cleaner_id'));
		$apply_id = intval($request->getParam('apply_id'));
		if (empty($user_id) || empty($cleaner_id) || empty($apply_id))
			$this->failed('参数错误');
		$applyLog = UserApplyLog::model()->findByPk($apply_id);
		if (empty($applyLog))
			$this->failed('申请不存在');
		if ($applyLog->apply_uid != $user_id || $applyLog->cleaner_id != $cleaner_id)
			$this->failed('信息不符, 非法访问');
		if ($applyLog->status != UserApplyLog::APPLY_STATUS_APPLY)
			$this->failed('该申请已经审核过了');
		if (!UserCleaner::model()->checkIsCharger(Yii::app()->user->id, $cleaner_id))
			$this->failed('不是主账户没有审核权限');
		$userCleaner = UserCleaner::model()->findByAttributes(array('user_id' => $user_id, 'cleaner_id' => $cleaner_id));
		if (empty($userCleaner))
		{
			UserApplyLog::model()->deleteByPk($apply_id);
			$this->failed('无法通过申请, 控制不存在');
		}
		UserApplyLog::model()->updateByPk($apply_id, array('status' => UserApplyLog::APPLY_STATUS_REFUSE, 'verify_time' => time(), 'verify_uid' => Yii::app()->user->id));
		//删除
		UserCleaner::model()->deleteByPk($userCleaner->id);
		$this->success();
	}


	/**
	 * 查看全部绑定(通过的 未通过的), 只有主账户可以查询
	 * Enter description here ...
	 * /apply/index/
	 * @param cleaner_id
	 * @param token
	 * @param sign
	 */
	public function actionGetBindUser()
	{
		//$this->checkSign(array('token', 'cleaner_id', 'client_id'));
		$cleaner_id = trim(Yii::app()->request->getParam('cleaner_id'));
		//判断是否成功绑定了该净化器
		$userCleaner = UserCleaner::model()->checkHasBind(Yii::app()->user->id, $cleaner_id);
		if(!$userCleaner)
			$this->failed('没有权限查看');
		$is_charger = $userCleaner->is_charger == UserCleaner::IS_CHARGER_YES ? true : false;
		$data = array();
		//获取已经绑定的
		$result = UserCleaner::model()->findAll(array(
			'condition' => "cleaner_id = '{$cleaner_id}' AND is_verify = ".UserCleaner::IS_VERIFY_YES,
			'order' => 'is_charger DESC',
			'select' => 'user_id,is_charger,is_verify'
		));
		if($result)
		{
			foreach ($result as $value)
			{
				$data[] = array(
					'user_id'       => $value->user->id,
					'user_mobile'   => $value->user->mobile,
					'is_charger'    => $value->is_charger,
					'apply_id'		=> 0,
					'apply_status'	=> UserApplyLog::APPLY_STATUS_PASS,
				);
			}
		}
		//获取申请的
		if($is_charger)
		{
			$result = UserApplyLog::model()->with('user')->findAllByAttributes(array('cleaner_id' => $cleaner_id,'status' => UserApplyLog::APPLY_STATUS_APPLY));
			if (!empty($result))
			{
				foreach ($result as $value)
				{
					$data[] = array(
						'user_id'       => $value->user->id,
						'user_mobile'   => $value->user->mobile,
						'is_charger'    => UserCleaner::IS_CHARGER_NO,
						'apply_id'		=> $value->id,
						'apply_status'	=> $value->status,
					);
				}
			}
		}
		$return['is_charger'] = $is_charger ? true : false;
		$return['users'] = $data;
		$this->success($return);
	}

	/**
	 * 解除绑定
	 */
	public function actionRemoveBind()
	{
		$request = Yii::app()->request;
		$user_id = intval($request->getParam('user_id', 0));
		$cleaner_id = trim($request->getParam('cleaner_id'));
		if (empty($user_id) || empty($cleaner_id))
			$this->failed('参数错误');
		if (UserCleaner::model()->checkIsCharger(Yii::app()->user->id, $cleaner_id))
		{
			//如果是解除自己
			if ($user_id == Yii::app()->user->id)
			{
				//判断是否还有其他管理账号
				$condition = "cleaner_id = '{$cleaner_id}' AND is_verify = " . UserCleaner::IS_VERIFY_YES . ' AND user_id != ' . $user_id;
				$result = UserCleaner::model()->findAll($condition);
				if ($result)
				{
					//判断是否还有其他主账号，如果没有，需要升级一个主账号
					$has_other_master_charger = false;
					$slave_charger = array();
					foreach ($result as $value) {
						if ($value->is_charger == UserCleaner::IS_CHARGER_YES)
						{
							//可以解绑
							$has_other_master_charger = true;
							break;
						} else
						{
							$slave_charger[] = array(
								'user_id' => $value->user->id,
								'user_mobile' => $value->user->mobile,
							);
						}
					}
					if (!$has_other_master_charger)
					{
						$msg = '您需要先升级一个副账号为主账号后才能解绑此账号';
						$this->specialResponse(2, $msg, $slave_charger);//特殊格式返回
						//$this->failed('您需要先升级一个副账号为主账号后才能解绑此账号');
					}
				}
			}
		}else
		{
			if ($user_id != Yii::app()->user->id)
				$this->failed('只有主账户才能进行解绑操作');
		}
		$userCleaner = UserCleaner::model()->checkHasBind($user_id,$cleaner_id);
		if ($userCleaner)
			//删除
			UserCleaner::model()->deleteByPk($userCleaner->id);
		$this->success();
	}

	/**
	 * 获取副账号
	 */
	public function actionGetSlaveCharger()
	{
		$cleaner_id = trim(Yii::app()->request->getParam('cleaner_id'));
		if (!UserCleaner::model()->checkIsCharger(Yii::app()->user->id, $cleaner_id))
			$this->failed('没有权限进行此操作');
		$data = array();
		$result = UserCleaner::model()->findAll(array(
			'condition' => "cleaner_id = '{$cleaner_id}' AND is_verify = ".UserCleaner::IS_VERIFY_YES.' AND is_charger = '.UserCleaner::IS_CHARGER_NO,
			'order' => 'is_charger DESC',
			'select' => 'is_charger,is_verify'
		));
		if($result)
		{
			foreach ($result as $value)
			{
				$data[] = array(
					'user_id'       => $value->user->id,
					'user_mobile'   => $value->user->mobile,
				);
			}
		}
		$this->success($data);
	}

	/**
	 * 升级主账号
	 */
	public function actionChangeCharger()
	{
		$user_id = intval(Yii::app()->request->getParam('user_id', 0));
		$cleaner_id = trim(Yii::app()->request->getParam('cleaner_id'));
		if(!$user_id || !$cleaner_id)
			$this->failed('参数错误');
		if (!UserCleaner::model()->checkIsCharger(Yii::app()->user->id, $cleaner_id))
			$this->failed('没有权限进行此操作');
		//判断$user_id是否有管理权限
		$userCleaner = UserCleaner::model()->findByAttributes(array('user_id' => $user_id, 'cleaner_id' => $cleaner_id));
		if (!$userCleaner)
			$this->failed('该账号还没有该净化器管理权限呢');
		//解除绑定当前账号
		UserCleaner::model()->deleteAllByAttributes(array('user_id' => Yii::app()->user->id, 'cleaner_id' => $cleaner_id));
		UserCleaner::model()->updateByPk($userCleaner->id,array('is_charger' => UserCleaner::IS_CHARGER_YES));
		$this->success();
	}
	
}