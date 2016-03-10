<?php

/**
 * 推送
 * @author zhoujianjun
 *
 */
class EPush extends CApplicationComponent
{
	
	/**
	 * 是否测试
	 * @var unknown
	 */
	protected $debug = false;
	
	/**
	 * 如果用户不在线,是否重新推送
	 * @var unknown
	 */
	protected $repush = false;
	
	
	/**
	 * 百度开发者账号申请的APP_ID
	 * @var unknown
	 */
	const APP_ID = 3499098;
	
	
	/**
	 * Android Push API_KEY
	 * @var unknown
	 */
	const API_KEY = 'ANL9bEA8VFxdx71HZdbjwNzV';
	
	
	/**
	 * Android Push SECRET_KEY
	 * @var unknown
	 */
	const SECRET_KEY = 'tYGASEBav1cafjFYPwfFBOr2Gmvf1Gi2';
	
	/**
	 * Android Push Channel
	 * @var unknown
	 */
	protected $channel = FALSE;
	
	/**
	 * IOS Push
	 * @var unknown
	 */
	protected $fp = FALSE;
	
	/**
	 * 推送的用户
	 * @var unknown
	 */
	protected $user = FALSE;
	
	
	/**
	 * (non-PHPdoc)
	 * @see CApplicationComponent::init()
	 */
	public function init()
	{
		
		// Baidu Android 推送 
		require dirname( __FILE__) . '/Channel.class.php';
		$this->channel = new Channel(self::API_KEY, self::SECRET_KEY);
		
		// IOS推送
		if ($this->debug)
		{
			// 测试环境
			$pem = dirname(__FILE__). '/ck.pem';
			$url = 'ssl://gateway.sandbox.push.apple.com:2195';
		}
		else 
		{
			// 正式
			$pem = dirname(__FILE__). '/ck_line.pem';
			$url = 'ssl://gateway.push.apple.com:2195';
		}
		$passphrase = '123456';
		$ctx = stream_context_create();
		stream_context_set_option($ctx, 'ssl', 'local_cert', $pem);
		stream_context_set_option($ctx, 'ssl', 'passphrase', $passphrase);
		$fp = stream_socket_client($url, $errno, $errstr, 60, STREAM_CLIENT_CONNECT, $ctx);
		if (!$fp)
			Yii::log("Failed to connect APNS server, ErrorNo: $errno, ErrorMsg: $errstr", 'push');
		else 
			$this->fp = $fp;

	}
	
	
	/**
	 * 推送接口
	 * @param unknown $uid
	 * @param array $data 发送的数据数组,必须包含的key为 title , content, 其余的为自定义字段
	 */
	public function push($uid, $data=array(), $repush=false)
	{
		// 测试
		$user = Token::model()->findByAttributes(array('user_id' => $uid));
		if (empty($user))
			return false;
		
		$this->user = $user;
		
		// 预留
		$this->repush = $repush;
		
		// 当前在线的用户推送
		if (!empty($user->ios_device_token))
		{
			$this->pushIos($user->ios_device_token, $data);
		}
		
		if (!empty($user->android_user_id) && !empty($user->android_channel_id))
		{
			$this->pushAndroid($user->android_user_id, $user->android_channel_id, $data);
		}
	}
	
	/**
	 * Android推送
	 */
	protected function pushAndroid($user_id, $channel_id, $data=array())
	{
		// message_type 消息类型 0消息(透传)1通知
		$optional = array(
			Channel::MESSAGE_TYPE => 1,
			Channel::CHANNEL_ID => $channel_id,
			Channel::USER_ID => $user_id
		);
		
		$message = $this->getAndroidMessage($data);
		
		//echo $message;die;
		
		// 单人推送 pushType=1
		$result = $this->channel->pushMessage(1, $message, time(), $optional);
		
		if ($result === false)
		{
			// 推送失败,记录失败日志
			$error = 'User:' . $this->user->id . 'Android Push Error, ErrorMsg:' . $this->channel->errmsg();
			Yii::log($error, 'push');
		}
	}
	
	/**
	 * IOS推送
	 */
	protected function pushIos($device_token, $data=array())
	{
		if ($this->fp)
		{
			$message = $this->getIosMessage($device_token, $data);
			$result = fwrite($this->fp, $message, strlen($message));
			if (!$result)
			{
				$error = 'User:' . $this->user->id . 'IOS Push Error';
				Yii::log($error, 'push');
			}
		}
	}
	
	/**
	 * 生成Android推送的消息内容
	 * @param unknown $data
	 */
	protected function getAndroidMessage($data)
	{
		$custom = '';
		foreach ($data as $key => $value)
		{
			if ($key != 'title' && $key != 'content')
			{
				// 非title 非content的字段为自定义字段
				$custom .= '"' . $key .'":"' . $value . '",' . PHP_EOL;
			}
		}
		$custom = substr($custom, 0, strlen($custom)-strlen(PHP_EOL)-1);
		$message='{
			"title" : "'.$data['title'].'",
			"description": "'.$data['content'].'",
			"aps":{"alert":"'.$data['content'].'"},
			"custom_content": {' .$custom.
							  '}
		}';
		return $message;	
	}
	
	/**
	 * 生成IOS推送的消息内容
	 * @param unknown $data
	 */
	protected function getIosMessage($device_token, $data)
	{
		
		$body['aps'] = array(
			'alert' => $data['content'],
			'sound' => 'default',
			'badge' => 1,
			'title' => $data['title'],
		);
		
		// title 和 infoid 为自定义字段
		foreach ($data as $key => $value)
		{
			if ($key != 'title' && $key != 'content')
			{
				// 非title 非content的字段为自定义字段
				$body['aps'][$key] = $value;
			}
		}
		
		// Encode the payload as JSON
		$payload = json_encode($body);
		// Build the binary notification
		$message = chr(0) . pack('n', 32) . pack('H*', $device_token) . pack('n', strlen($payload)) . $payload;
		return $message;
	}
}