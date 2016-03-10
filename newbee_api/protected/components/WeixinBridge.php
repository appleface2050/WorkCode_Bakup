<?php

class WeixinBridge
{
    const TOKEN_CACHE_KEY = 'A1234567890A2';//weixinx_token
    const JS_TICKET_CACHE_KEY = 'weixinx_js_ticket';
    const TOKEN_EXPIRE_TIME = 7000;
    const WX_RET_OK = 0;

	/**
	 * 发送客服消息(48小时内和微信公众号交互过)
	 * @param string openid 发送的用户的openid
	 * @param string $message 发送的消息
	 */
	public static function sendServiceMessage($openid,$message)
	{
		$accessToken = self::getAccessToken();
		if($accessToken)
		{
			$url = 'https://api.weixin.qq.com/cgi-bin/message/custom/send?access_token='.$accessToken;
			$data = array(
				'touser' => $openid,
				'msgtype' => 'text',
				'text' => array(
					'content' => $message		
				)
			);
			$result = Yii::app()->curl->post($url,urldecode(json_encode($data)));
			if(!empty($result))
			{
				$result = json_decode($result,true);
				return $result;
				//print_r($result);
			}
		}
	}
	
	/**
	 * 发送客服图文消息(48小时内和微信公众号交互过)
	 * @param string openid 发送的用户的openid
	 * @param array  msgarr 图片消息  二维数组
	 * @param msg_arr = array(
	 *                     array(
	 *                         'title'=>'title',
	 *                         'description'=>'description',
	 *                         'url'=>'url',
	 *                         'picurl'=>'picurl',
	 *                     ),
	 *                     ......
	 *                 )
	 */
	public static function sendServiceImgMsg($openid,$msg_arr = array())
	{
	    $accessToken = self::getAccessToken();
	    if($accessToken)
	    {
	        foreach($msg_arr as $val){
	        	if(!is_array($val)){
	        		return false;
	        	}
	        }
	        $url = 'https://api.weixin.qq.com/cgi-bin/message/custom/send?access_token='.$accessToken;
	        $data = array(
	            'touser' => $openid,
	            'msgtype' => 'news',
	            'news' => array(
	                'articles' => $msg_arr
	            )
	        );
	        $result = Yii::app()->curl->post($url,urldecode(json_encode($data)));
	        if(!empty($result))
	        {
	            $result = json_decode($result,true);
	            //Util::writeSlefLog('发送图文推送消息：'.var_export($result,true));
	            //print_r($result);
	        }
	    }
	}
	
	/**
	 * 创建菜单接口
	 * @param array $buttons 要创建的菜单数组
	 */
	public static function createMenu($buttons=array())
	{
		$accessToken = self::getAccessToken();
		if($accessToken)
		{
			$url = 'https://api.weixin.qq.com/cgi-bin/menu/create?access_token='.$accessToken;
			$post = array('button'=>$buttons);
			$result = Yii::app()->curl->post($url,urldecode(json_encode($post)));
			if(!empty($result))
			{
				$result = json_decode($result,true);
				print_r($result);
			}
		}
	}
	/**
	 * 删除菜单接口
	 */
	public static function deleteMenu()
	{
		$accessToken = self::getAccessToken();
		if($accessToken)
		{
			$url = 'https://api.weixin.qq.com/cgi-bin/menu/delete?access_token='.$accessToken;
			$result = Yii::app()->curl->get($url);
			print_r(json_decode($result,true));
		}
	}
	
	/**
	 * 回复文本消息
	 * @param string $from  openid
	 * @param string $to    openid
	 * @param string $content
	 */
	public static function responseTextMessage($from,$to,$content)
	{
		$formatData = array(
			'ToUserName' => $to,
			'FromUserName' => $from,
			'CreateTime' => time(),
			'MsgType' => 'text',
			'Content' => $content	
		);
		return self::getXml($formatData);
	}
	
	/**
	 * 转接多客服消息
	 * @param string $from  openid
	 * @param string $to    openid
	 */
	public static function responseTransferCustomerService($from,$to,$kefu='')
	{
	    $formatData = array(
	            'ToUserName' => $to,
	            'FromUserName' => $from,
	            'CreateTime' => time(),
	            'MsgType' => 'transfer_customer_service',
	    );
	    if(!empty($kefu)){
	        $formatData['TransInfo'] = array('KfAccount'=>'testLong@HTJ_XRomm');
	    }
	    return self::getTransferXml($formatData);
	}
	
	/**
	 * 回复图文消息
	 * @param string $from
	 * @param string $to
	 * @param array $data,图文消息数组,需要包含(title,
	 */
	public static function responseTextImgMessage($from,$to,$data)
	{
		$total = count($data);
		if($total>0) 
		{
			$formatData = array(
				'ToUserName' => $to,
				'FromUserName' => $from,
				'CreateTime' => time(),
				'MsgType' => 'news',
				'ArticleCount' => $total,
				'Articles' => $data	
			);
			return self::getXml($formatData);
		}
	}
	/**
	 * 获取AccessToken
	 */
	private static function getAccessToken()
	{
	    $access_token_file = dirname(__FILE__)."/access_token.json";
	    if(!is_file($access_token_file)){
	        file_put_contents($access_token_file, json_encode(array('expire_time'=>0)));
	    }
	    $data = json_decode(file_get_contents($access_token_file), true);
	    //Util::writeSlefLog('token_access:'.date('Y-m-d H:i:s').var_export($data,true));
	    if ($data['expire_time'] < time()) {
	        $url = "https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid=".Yii::app()->params['weixin']['appId']."&secret=".Yii::app()->params['weixin']['appSecret'];
	        $res = json_decode(Yii::app()->curl->get($url), true);
	        $access_token = $res['access_token'];
	        if ($access_token) {
	            $data['expire_time'] = time() + 7000;
	            $data['access_token'] = $access_token;
	            $fp = fopen($access_token_file, "w");
	            fwrite($fp, json_encode($data));
	            fclose($fp);
	        }
	    } else {
	        $access_token = $data['access_token'];
	    }
	    return $access_token;
	}

	public static function getAT()
	{
		return self::getAccessToken();
	}

	
	/**
	 * xml格式输出
	 * @param array $formatData 格式化后的数组数据
	 * @return string
	 */
	private static function getXml($formatData=array())
	{
		$xml = '<xml>' . PHP_EOL;
		$tpl = "	<%s><![CDATA[%s]]></%s>";
		foreach ($formatData as $key => $value)
		{
			if(is_array($value))
			{
				$xml .= '	<'.$key.'>' . PHP_EOL;
				foreach ($value as $item)
				{
					$xml .= '		<item>' . PHP_EOL;
					foreach ($item as $k => $v)
					{
						$k = ucfirst($k);
						$xml .= '		' . sprintf($tpl, $k, $v, $k) . PHP_EOL;
					}
					$xml .= '		</item>' . PHP_EOL;
				}
				$xml .= '	</'.$key.'>' . PHP_EOL;
			}else
			{
				if(is_numeric($value))
				{
					$xml .= '	' . sprintf("<%s>%d</%s>", $key, $value, $key) . PHP_EOL;
				}else 
				{
					$xml .=  sprintf($tpl,$key, $value, $key) . PHP_EOL;
				}
			}
		}
		$xml .= '</xml>';
		return $xml;
	}
	
	/**
	 * 多客服xml格式输出
	 * @param array $formatData 格式化后的数组数据
	 * @return string
	 */
	private static function getTransferXml($formatData=array())
	{
	    $xml = '<xml>' . PHP_EOL;
	    $tpl = "	<%s><![CDATA[%s]]></%s>";
	    foreach ($formatData as $key => $value)
	    {
	        if(is_array($value))
	        {
	            $xml .= '	<'.$key.'>' . PHP_EOL;
	            foreach ($value as $item_key=>$item)
	            {
                    $xml .= '		' . sprintf($tpl, $item_key, $item, $item_key) . PHP_EOL;
	            }
	            $xml .= '	</'.$key.'>' . PHP_EOL;
	        }else
	        {
	            if(is_numeric($value))
	            {
	                $xml .= '	' . sprintf("<%s>%d</%s>", $key, $value, $key) . PHP_EOL;
	            }else
	            {
	                $xml .=  sprintf($tpl,$key, $value, $key) . PHP_EOL;
	            }
	        }
	    }
	    $xml .= '</xml>';
	    return $xml;
	}
}