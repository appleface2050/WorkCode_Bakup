<?php

class Weixin extends CApplicationComponent
{
	
	/**
	 * 用户
	 * @var unknown
	 */
	private static $user = null;

	/**
	 * 接收文本消息
	 * @param array $param 微信传递过来的所有参数
	 */
	public static function handelTextImgMsg($param=array())
	{
		$content = $param['Content'];
		$data = array();
		switch($content)
		{
			case '外卖':
				$data = array(
					array(
						'Title' => '黄太吉红包',
						'PicUrl' => 'http://image.mrfood.cc/weixin/waimai.jpg',
						'Url' => 'http://mp.weixin.qq.com/s?__biz=MjM5MDY0MDA5NQ==&mid=211792092&idx=1&sn=b64c1a3e641bee48320d362c3b811cc6&scene=1&srcid=09248j83YmAF2NAlvO0r5uvG&key=2877d24f51fa53847498f1a4c9e45f36b6fa148b8b11f2679f64802456b20addc02aede702e29f27108056ab31ca259c&ascene=1&uin=MTU5NTAxMzkwOA%3D%3D&devicetype=Windows+7&version=6102002a&pass_ticket=vWCW0p8cmf%2F9dh%2Fcw0p75NMoDtGvxMeVeaXPft2qj%2BHRLeY56VTavnstYpn5rIs6',
					)
				);
				break;
		}
		return $data;
	}
	
	/**
	 * 接收文本消息
	 * @param array $param 微信传递过来的所有参数
	 */
	public static function handelTextMsg($param=array())
	{
	    $content = $param['Content'];
	    $data = array();
	    switch ($content)
	    {
	    	case '游戏':
	    	    return '猛戳赢大奖 <a href="http://www.sangebaba.com/game_wuml/index.php">http://www.sangebaba.com/game_wuml/index.php</a>';
	    	    break;
	    }
	    
	    return '请查证后再试。';
	}
	

	/**
	 * 关注公众号事件
	 * @param array $param 微信传递过来的所有参数
	 * @return string 关注公众号后返回的信息
	 */
	public static function handleSubscribeEvent($param=array())
	{
				$str = <<< EOF
因爱而生，公益助力。三个爸爸儿童专用空气净化器，帮助更多有孩子的家庭，让我们携起手来用“行动”让孩子感受到你对他的爱。
EOF;
 		return $str;
	}
	
	public static function handleClickEvent($param=array())
	{
	    $key = $param['EventKey'];
		$data = null;
	    switch($key)
	    {
			case 'SERVICE_PHONE':
				$data = <<< EOF
    有两种方式能为您解决问题：
1.有疑问烦请您致电三个爸爸客服热线：4007000909 客服老师会给您解答您关心的问题，谢谢。
2.我们现已开通三个爸爸客服微信号：sangebabakefu02 ，关注此微信号，将会有专业客服老师一对一给您解决你所关心的问题。（工作时间：早9:00-晚18:00）
EOF;
				break;

			case 'STORE_ADDRESS':
				$data = array(
					array(
						'Title' => '门店地址',
						'PicUrl' => 'http://image.mrfood.cc/weixin/address.jpg',
						'Url' => 'http://mp.weixin.qq.com/s?__biz=MjM5MDY0MDA5NQ==&mid=211629477&idx=1&sn=b6b3ca8c3ea7b1b9d83cfedd122e3cba#rd',
					)
				);
				break;
	    }
		return $data;
	}

	
	/**
	 * 获取公众号底部导航菜单
	 */
	
	public static function getMenus()
	{
		$app = Yii::app();
		$host = Yii::app()->request->getHostInfo();
		$buttons = array(

		    array(
		        'name' => urlencode('微购物'),
		        'sub_button' => array(
					array(
						'type' => 'view',
						'name' => urlencode('智能GO'),
						'url'  => 'http://shop501.in168.com/weixinweb/224/index?&state=123&opid=ow7Tcw3yMt-3-m-adUAM6bjjHISE#mp.weixin.qq.com'
					),
					array(
						'type' => 'view',
						'name' => urlencode('京东商城'),
						'url'  => 'http://wq.jd.com/mshop/gethomepage?venderId=1000002535'
					),
					array(
						'type' => 'view',
						'name' => urlencode('天猫旗舰店'),
						'url'  => 'http://sangebaba.m.tmall.com/'
					),
		        )
		    ),
		    
		    array(
		        'name' => urlencode('实用功能'),
		        'sub_button' => array(
					array(
						'name' => urlencode('客服热线'),
						'type' => 'click',
						'key'  => 'SERVICE_PHONE'
					),
					array(
						'name' => urlencode('APP下载'),
						'type' => 'view',
						'url'  => 'http://www.sangebaba.com/responsive/app.html'
					),
                    array(
                        'name' => urlencode('今日空气'),
						'type' => 'view',
						'url'  => 'http://www.soupm25.com/city/beijing.html'
                    )
                )
		    ),

			array(
				'name' => urlencode('最新活动'),
				'sub_button' => array(
					array(
						'name' => urlencode('真GO实惠'),
                        'type' => 'view',
                        'url'  => 'http://mp.weixin.qq.com/s?__biz=MzAxNTAxNzM0OA==&mid=207914039&idx=1&sn=9c0dd6b34aec151021561e523e33e241#rd'
					),
                    array(
                        'name' => urlencode('京东双节提前GO'),
                        'type' => 'view',
                        'url'  => 'http://sale.jd.com/m/act/Wd4Uc2jgrGIwKi.html'
                    )
				)
			),
		);
		return $buttons;
	}
	
	
}