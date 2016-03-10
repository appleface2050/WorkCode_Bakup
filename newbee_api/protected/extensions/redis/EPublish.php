<?php

/**
 * 发布
 * @author zhoujianjun
 *
 */
class EPublish extends CApplicationComponent
{
	
	/**
	 * 净化器参数变更的channel
	 * @var unknown
	 */
	const CHANNEL_PARAMETER_CHANGE = 'PARAMETER_CHANGE';
	
	/**
	 * 硬件固件升级
	 * @var unknown
	 */
	const CHANNEL_UPGRADE = 'UPGRADE_PROGRAM';
	
	/**
	 * 在某channel发布消息
	 * @param string $channel
	 * @param string $message
	 */
	protected function publish($channel, $message) 
	{
		Yii::app()->redis->publish($channel, $message);
	}
	
	/**
	 * 净化器参数变更时调用
	 * @param unknown $message
	 */
	public function publishParameter($message) 
	{
		$this->publish(self::CHANNEL_PARAMETER_CHANGE, $message);
	}
	
	
	/**
	 * 客户端申请 净化器硬件程序升级
	 * @param unknown $message
	 */
	public function publishUpgrade($message)
	{
		$this->publish(self::CHANNEL_UPGRADE, $message);
	}
}