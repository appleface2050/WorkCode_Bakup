<?php

/**
 * 推送
 * @author zhoujianjun
 *
 */
class PushController extends RestController
{
	
	
	
	/**
	 * 推送的配置,ios为device_token  android为user_id 和 channel_id
	 */
	public function actionParam()
	{
		$request = Yii::app()->request;
		$device_token = $request->getParam('device_token', '');
		if (!empty($device_token))
		{
			// IOS 更新device_token
			Token::model()->updateByPk(Yii::app()->user->id, array('ios_device_token' => $device_token));
		} 
		else
		{
			$user_id = $request->getParam('user_id');
			$channel_id = $request->getParam('channel_id');
			if (empty($user_id) || empty($channel_id))
				throw new CHttpException(400, '参数错误');
			Token::model()->updateByPk(Yii::app()->user->id, array('android_user_id' => $user_id, 'android_channel_id' => $channel_id));
		}
		$this->success();
	}
	
	
}