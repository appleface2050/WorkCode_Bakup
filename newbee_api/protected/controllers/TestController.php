<?php

class TestController extends CController
{
	
	

	/**
	 * 有PM25数据的城市
	 * @var unknown
	 */
	public $citys = array("三亚","三门峡","上海","东莞","东营","中山","临安","临汾","临沂","丹东","丽水","义乌","乌鲁木齐","九江","乳山","云浮","佛山","保定","克拉玛依","兰州","包头","北京","北海","南京","南充","南宁","南昌","南通","即墨","厦门","句容","台州","合肥","吉林","吴江","呼和浩特","咸阳","哈尔滨","唐山","嘉兴","嘉峪关","大同","大庆","大连","天津","太仓","太原","威海","宁波","安阳","宜兴","宜宾","宜昌","宝鸡","宿迁","富阳","寿光","岳阳","常州","常德","常熟","平度","平顶山","广州","库尔勒","廊坊","延安","开封","张家口","张家港","张家界","徐州","德州","德阳","惠州","成都","扬州","承德","抚顺","拉萨","招远","揭阳","攀枝花","文登","无锡","日照","昆山","昆明","曲靖","本溪","杭州","枣庄","柳州","株洲","桂林","梅州","武汉","汕头","汕尾","江门","江阴","沈阳","沧州","河源","泉州","泰安","泰州","泸州","洛阳","济南","济宁","海口","海门","淄博","淮安","深圳","清远","温州","渭南","湖州","湘潭","湛江","溧阳","滨州","潍坊","潮州","烟台","焦作","牡丹江","玉溪","珠海","瓦房店","盐城","盘锦","石嘴山","石家庄","福州","秦皇岛","章丘","绍兴","绵阳","聊城","肇庆","胶南","胶州","自贡","舟山","芜湖","苏州","茂名","荆州","荣成","莱州","莱芜","莱西","菏泽","营口","葫芦岛","蓬莱","衡水","衢州","西宁","西安","诸暨","贵阳","赤峰","连云港","遵义","邢台","邯郸","郑州","鄂尔多斯","重庆","金华","金坛","金昌","铜川","银川","锦州","镇江","长春","长沙","长治","阳江","阳泉","青岛","鞍山","韶关","马鞍山","齐齐哈尔");
	
	
	/**
	 * 获取一个城市的全部检测点的PM25值
	 * @var unknown
	*/
	protected $url = 'http://www.pm25.in/api/querys/aqi_ranking.json?token=';
	
	/**
	 * token
	 * @var unknown
	 */
	protected $token = '5j1znBVAsnSf5xQyNQyq';
	
	
	public function actionSign()
	{
		$secret = '#$cleaner&*%$app';
		
		// 发短信验证码
// 		$data = array(
// 			'client_id' => 'AIRCLEANERNEEBEER',
// 			'mobile' => '573932979@qq.com'	
// 		);
		
		// 检验验证码
		$data = array(
			'mobile' => '573932979@qq.com',
			//'code' => 2776,
			'client_id' => 'AIRCLEANERNEEBEER',
		);

		// 登录
// 		$data = array(
// 			'mobile' => '14000000000',
// 			'password' => 'sgbbsgbb',
// 			'platform' => 'Iphone',
// 			'client_id' => 'AIRCLEANERNEEBEER'
// 		);
		// 注册
		$data = array(
			'mobile' => '573932979@qq.com',
			'password' => '111111',
			'nickname' => '客户',
			'app_version' => '1.0',
			'platform' => 'android',
			'client_id' => 'AIRCLEANERNEEBEER'
		);
		
		// 重置token
		$data = array(
			'token' => '3eb399143ebcdd75cb5aaab9913b65f4',
			'client_id' => 'AIRCLEANERNEEBEER'
		);

		// 查看申请
		$data = array(
			'token' => 'b37f577a8baf3625ac8b3f02d9cb8224',
			'client_id' => 'AIRCLEANERNEEBEER',
			'cleaner_id' => '60C5A8605483'
		);
		
		// 申请通过
		$data = array(
			'token' => 'b37f577a8baf3625ac8b3f02d9cb8224',
			'user_id' => 4,
			'client_id' => 'AIRCLEANERNEEBEER',
			'cleaner_id' => '60C5A8605483',
			'apply_id' => 1
		);
		
		ksort($data);
		
		$join = array();
		foreach ($data as $key => $value)
		{
			$join[] = "$key=$value";
		}
		
		
		//echo join('&', $join);die;
		//echo join('&', $join).$secret;die;
	
		
		echo md5(join('&', $join).$secret);
	}
	
	
	
	public function actionRecord()
	{
		$result = json_decode(Yii::app()->curl->get($this->url . $this->token), true);
		if (!isset($result['error']))
		{
			foreach ($result as $value)
			{
				$city = $value['area'];
				
				// AQI 历史
				$history = new AqiHistory();
				$history->attributes = array(
					'city' => $city,
					'name' => $city,
					'aqi' => $value['aqi'],
					'time_point' => $value['time_point']
				);
				$history->save();
				
				// AQI 最新的
				$current = AqiIndex::model()->findByAttributes(array('name'=>$city));
				if (!empty($current))
				{
					$current->attributes = array(
						'aqi' => $value['aqi'],
						'update_time' => time(),
						'time_point' => $value['time_point']
					);
					$current->save();
				}
			}
		}
	}
	
	
	public function actionJson()
	{
		$data = array(
			array(
				'day' => '1',
				'start_time' => '15:00',
				'end_time' => '16:00',
				'open' => 1
			),
			array(
				'day' => '5',
				'start_time' => '12:10',
				'end_time' => '18:30',
				'open' => 1
			)
		);
		
		$set = json_decode('[{"open":1,"end_time":"23:59","start_time":"17:58","day":""},{"open":1,"end_time":"18:57","start_time":"16:57","day":""},{"open":1,"end_time":"23:59","start_time":"00:00","day":"5"},{"open":1,"end_time":"23:59","start_time":"00:00","day":"6"},{"open":1,"end_time":"23:59","start_time":"00:00","day":"6"},{"open":1,"end_time":"23:59","start_time":"00:00","day":"4,5"}]', true);
		print_r($set);die;
		
		echo json_encode($data);
		
	}
	
	public function actionPm()
	{
		
		$data = array(
			'title' => '三个爸爸',
			'content' => '滤芯快没命了',
			'info_id' => 2,
			'cleaner_id' => '1'
		);
		
		Yii::app()->push->push(4, $data);
		
		
		
		
		$d = Yii::app()->location->getCity('121.27175', '31.02668');
		
		//$d = Yii::app()->location->getPm('北京');
		echo $d;
		
// 		$url = 'http://www.pm25.in/api/querys/pm2_5.json?token=5j1znBVAsnSf5xQyNQyq&city=北京';
		
// 		$data = Yii::app()->curl->get($url);
		
// 		print_r(json_decode($data));
		
	}
	
	public function actionTimeset()
	{
		
		//echo json_encode(unserialize('a:4:{i:0;a:4:{s:4:"open";i:1;s:8:"end_time";s:5:"23:59";s:10:"start_time";s:5:"22:30";s:3:"day";s:5:"1,2,3";}i:1;a:4:{s:4:"open";i:1;s:8:"end_time";s:5:"23:59";s:10:"start_time";s:5:"12:10";s:3:"day";s:1:"5";}i:2;a:4:{s:4:"open";i:1;s:8:"end_time";s:5:"20:00";s:10:"start_time";s:5:"05:00";s:3:"day";s:9:"1,2,3,4,5";}i:3;a:4:{s:4:"open";i:1;s:8:"end_time";s:5:"23:59";s:10:"start_time";s:5:"00:00";s:3:"day";s:9:"1,2,3,4,5";}}'));
		
		//echo json_encode(array());
		
		//die;
		
		$data = array(
			array(
				'day' => '1',
				'start_time' => '07:00',
				'end_time' => '03:00',
				'open' => 1
			),
			array(
					'day' => '1',
					'start_time' => '21:30',
					'end_time' => '18:10',
					'open' => 1
			),
			array(
				'day' => '1',
				'start_time' => '09:00',
				'end_time' => '05:00',
				'open' => 1
			),
			array(
				'day' => '1',
				'start_time' => '15:00',
				'end_time' => '08:00',
				'open' => 1
			),
					
		);
		
		$d = '[{"end_time":"07:00","open":1,"day":"1","start_time":"03:00"},{"end_time":"21:30","open":1,"day":"1","start_time":"18:10"},{"end_time":"09:00","open":1,"day":"1","start_time":"05:00"},{"end_time":"15:00","open":1,"day":"1","start_time":"08:00"},{"day":"2","start_time":"17:14","end_time":"23:00"}]';
		
		print_r(json_decode($d, true));die;
		echo json_encode($data);die;
		CleanerStatus::model()->getFormatTimeset(json_encode($data));
		
		
	}
	
	/**
	 * 测试短信发送
	 */
	public function actionSms()
	{
		
		$mobile = '13911153235';
		$code = 1211;
		
		$d = Yii::app()->sms->send($mobile, $code);
		
		
		print_r($d);die;
	}
	
	public function actionTe()
	{
		
		$point_x = '121.271676';
		$point_y = '31.026760';
		$city = Yii::app()->location->getCity($point_x, $point_y);  //获取经纬度所在的城市
		
		echo $city;die;
		//Yii::log('test','info');
	}
	
	/**
	 * 生成测试序列号
	 */
	public function actionAuto()
	{
		set_time_limit(0);
		// 大机器的
		$pre = 't1';
		
		for($i=0; $i<200; $i++)
		{
			$qrcode = $pre . $i . rand(1000, 9999) . rand(1000, 9999);
			$model = new Cleaner();
			$model->attributes = array(
				'qrcode' => $qrcode,
				'cleaner_id' => '',
				'release_date' => '2014-10-10',
				'version' => '1',
				'add_time' => time(),
				'type' => 1
			);
			$model->save();
		}
		
		// 小机器
		$pre = 't2';
		for($i=0; $i<200; $i++)
		{
			$qrcode = $pre . $i . rand(1000, 9999) . rand(1000, 9999);
			$model = new Cleaner();
			$model->attributes = array(
				'qrcode' => $qrcode,
				'cleaner_id' => '',
				'release_date' => '2014-10-10',
				'version' => '1',
				'add_time' => time(),
				'type' => 1
			);
			$model->save();
		}
		
		// 内部测试
		$pre = 't3';
		for ($i=0; $i<100; $i++)
		{
			$qrcode = $pre . $i . rand(1000, 9999) . rand(1000, 9999);
			$model = new Cleaner();
			$model->attributes = array(
				'qrcode' => $qrcode,
				'cleaner_id' => '',
				'release_date' => '2014-10-10',
				'version' => '1',
				'add_time' => time(),
				'type' => 1
			);
			$model->save();
		}
		
		
	}
	
	/**
	 * 临时统计
	 */
	public function actionStat()
	{
		$sql = "SELECT count(user_id) AS total FROM user_token";
		$total = Yii::app()->db->createCommand($sql)->queryScalar();
		
		$n = 100;
		$j = ceil($total/100);
		$data = array(
			'android' => 0,
			'ios' => 0
		);
		for ($i=0; $i<$j; $i++)
		{
			$sql = "SELECT * FROM user_token LIMIT ".$i*$n.",".$n;
			$result = Yii::app()->db->createCommand($sql)->queryAll();
			foreach ($result as $value)
			{
				
				$platform = $value['platform'];
				$ios = stripos($platform, 'ios');
				if ($ios === false)
					$data['android']++;
				else 
					$data['ios']++;
			}
		}
		
		print_r($data);
		
	}

	public function actionEncrypt()
	{

		$str = self::authcode('1dsfsdf2e1234124','decode');
		echo $str.'<br>';
		echo self::authcode($str);
	}
	private static function authcode($string, $operation = 'DECODE', $key = '', $expiry = 0) {
		$ckey_length = 4;
		$key = md5($key != '' ? $key : 'CSLTOKENKEY');
		$keya = md5(substr($key, 0, 16));
		$keyb = md5(substr($key, 16, 16));
		$keyc = $ckey_length ? ($operation == 'DECODE' ? substr($string, 0, $ckey_length): substr(md5(microtime()), -$ckey_length)) : '';

		$cryptkey = $keya.md5($keya.$keyc);
		$key_length = strlen($cryptkey);

		$string = $operation == 'DECODE' ? base64_decode(substr($string, $ckey_length)) : sprintf('%010d', $expiry ? $expiry + time() : 0).substr(md5($string.$keyb), 0, 16).$string;
		$string_length = strlen($string);

		$result = '';
		$box = range(0, 255);

		$rndkey = array();
		for($i = 0; $i <= 255; $i++) {
			$rndkey[$i] = ord($cryptkey[$i % $key_length]);
		}

		for($j = $i = 0; $i < 256; $i++) {
			$j = ($j + $box[$i] + $rndkey[$i]) % 256;
			$tmp = $box[$i];
			$box[$i] = $box[$j];
			$box[$j] = $tmp;
		}

		for($a = $j = $i = 0; $i < $string_length; $i++) {
			$a = ($a + 1) % 256;
			$j = ($j + $box[$a]) % 256;
			$tmp = $box[$a];
			$box[$a] = $box[$j];
			$box[$j] = $tmp;
			$result .= chr(ord($string[$i]) ^ ($box[($box[$a] + $box[$j]) % 256]));
		}

		if($operation == 'DECODE') {
			if((substr($result, 0, 10) == 0 || substr($result, 0, 10) - time() > 0) && substr($result, 10, 16) == substr(md5(substr($result, 26).$keyb), 0, 16)) {
				return substr($result, 26);
			} else {
				return '';
			}
		} else {
			return $keyc.str_replace('=', '', base64_encode($result));
		}

	}
}