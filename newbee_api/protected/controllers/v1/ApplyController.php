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
	 * 查看全部申请(通过的 未通过的), 只有主账户可以查询
	 * Enter description here ...
	 * /apply/index/
	 * @param cleaner_id
	 * @param token
	 * @param sign
	 */
	public function actionIndex()
	{
		$this->checkSign(array('token', 'cleaner_id', 'client_id'));
		
		$cleaner_id = trim(Yii::app()->request->getParam('cleaner_id'));
		if (!UserCleaner::model()->checkIsCharger(Yii::app()->user->id, $cleaner_id))
			$this->failed('不是主账户没有审核权限');
		$data = array();
		$result = UserApplyLog::model()->with('user')->findAllByAttributes(array('cleaner_id' => $cleaner_id));
		if (!empty($result))
		{
			foreach ($result as $value)
			{
				$data[] = array(
					'user_id'       => $value->user->id,
					'user_nickname' => $value->user->nickname,
					'user_mobile'   => $value->user->mobile,
					'apply_time'    => $value->add_time,
					'apply_id'		=> $value->id,
					'apply_status'	=> $value->status,
				);
			}
		}
		$this->success($data);
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
		$this->checkSign(array('token', 'cleaner_id', 'client_id', 'user_id', 'apply_id'));
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
			$this->failed('没法通过申请, 控制不存在');
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
		if ($applyLog->user_id != $user_id || $applyLog->cleaner_id != $cleaner_id)
			$this->failed('信息不符, 非法访问');
		if ($applyLog->status != UserApplyLog::APPLY_STATUS_APPLY)
			$this->failed('该申请已经审核过了');
		if (!UserCleaner::model()->checkIsCharger(Yii::app()->user->id, $cleaner_id))
			$this->failed('不是主账户没有审核权限');
		$userCleaner = UserCleaner::model()->findByAttributes(array('user_id' => $user_id, 'cleaner_id' => $cleaner_id));
		if (empty($userCleaner))
			$this->failed('没法通过申请, 控制不存在');
		UserApplyLog::model()->updateByPk($apply_id, array('status' => UserApplyLog::APPLY_STATUS_REFUSE, 'verify_time' => time(), 'verify_uid' => Yii::app()->user->id));
		$this->success();
	}
	
	
}