<?php

/**
 * 发送短信
 * @author zhoujianjun
 *
 */
class Sms extends CApplicationComponent
{
	
	//protected $host = 'https://sandboxapp.cloopen.com';
	
	protected $host = 'https://app.cloopen.com';
	
	protected $port = 8883;
	
	/**
	 * 版本号
	 * @var unknown
	 */
	protected $SoftVersion = '2013-12-26';
	
	/**
	 * 账户id
	 * @var unknown
	 */
	protected $accountSid = '8a48b551473976010147437b66bb051d';
	
	/**
	 * token
	 * @var unknown
	 */
	protected $authToke = 'fb19559f95584bddb49066dd27f7d1fe';
	
	/**
	 * 应用appid
	 * @var unknown
	 */
	protected $appId = '8a48b5514739760101474503e9fa05d0';
	
	/**
	 * 模板id
	 * @var unknown
	 */
	protected $templateId = 2534;
	
	/**
	 * 时间戳
	 * @var unknown
	 */
	protected $batch = null;
	
	/**
	 * 包体格式
	 * @var unknown
	 */
	protected $bodyType = 'json';
	
	/**
	 * 验证码失效时间 15分钟
	 * @var unknown
	 */
	protected $expireTime = 15;
	
	
	public function init()
	{
		$this->batch = date("YmdHis");
	}
	
	
	/**
	 * 发送短信(只发送一个号码)
	 * @param unknown $mobile
	 * @param unknown $content
	 * @return array status 状态码1成功0失败,msg失败对应的消息
	 */
	public function send($mobile, $code)
	{
		$header = $this->getHeader();
		
		$url = $this->getUrl();
		
		$data= "{'to':'$mobile','templateId':$this->templateId,'appId':'$this->appId','datas':['".$code."','".$this->expireTime."']}";
		
		$result = $this->curl_post($url, $data, $header);
		
		$result = json_decode($result, true);
		
		if($result['statusCode'] == '000000')
			$data = array('status' => 1, 'msg' => '成功');
		else
			$data = array('status' => 0, 'msg' => $result['statusMsg']);
		return $data;
	}
	
	
	/**
	 * url
	 * @return string
	 */
	private function getUrl()
	{
		$sign = $this->getSign();
		return $this->host . ':' . $this->port . '/' . $this->SoftVersion . '/Accounts/'. $this->accountSid.'/SMS/TemplateSMS?sig=' .  strtoupper($sign); 
	}
	
	/**
	 * 返回签名字符串 规则(账户Id + 账户授权令牌 + 时间戳)
	 */
	private function getSign()
	{
		return md5($this->accountSid . $this->authToke . $this->batch);
	}
	
	/**
	 * http header
	 */
	private function getHeader()
	{
		// 生成授权：主帐户Id + 英文冒号 + 时间戳。
		$authen = base64_encode($this->accountSid . ":" . $this->batch);
		return array("Accept:application/$this->bodyType","Content-Type:application/$this->bodyType;charset=utf-8","Authorization:$authen");
	}
	
	/**
	 * 发起HTTPS请求
	 */
	function curl_post($url,$data,$header,$post=1)
	{
		//初始化curl
		$ch = curl_init();
		//参数设置
		$res= curl_setopt ($ch, CURLOPT_URL,$url);
		curl_setopt($ch, CURLOPT_SSL_VERIFYHOST, FALSE);
		curl_setopt($ch, CURLOPT_SSL_VERIFYPEER, FALSE);
		curl_setopt ($ch, CURLOPT_HEADER, 0);
		curl_setopt($ch, CURLOPT_POST, $post);
		if($post)
			curl_setopt($ch, CURLOPT_POSTFIELDS, $data);
		curl_setopt ($ch, CURLOPT_RETURNTRANSFER, 1);
		curl_setopt($ch,CURLOPT_HTTPHEADER,$header);
		$result = curl_exec ($ch);
		//连接失败
		if($result == FALSE){
			if($this->bodyType=='json'){
				$result = "{\"statusCode\":\"172001\",\"statusMsg\":\"网络错误\"}";
			} else {
				$result = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><Response><statusCode>172001</statusCode><statusMsg>网络错误</statusMsg></Response>";
			}
		}
	
		curl_close($ch);
		return $result;
	}
}