<?php

/**
 * 消息管理
 * @author zhoujianjun
 *
 */
class MessageController extends RestController
{
	
	
	/**
	 * 消息类列表
	 */
	public function actionIndex()
	{
		$timestamp = $this->getTimestamp();
		$data = Message::model()->getLatestMessage($timestamp);
		$this->success(array('timestamp'=>time(),'data'=>$data));
	}
	
	
	/**
	 * 获取时间戳
	 * @see MessageController::actionArticle() 等
	 */
	private function getTimestamp()
	{
		$timestamp = Yii::app()->request->getParam('timestamp',null);
		if(!empty($timestamp) && !is_numeric($timestamp))
			throw new CHttpException(400, '时间戳格式错误');
		return $timestamp;
	}
	
	
}