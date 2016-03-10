<?php

/**
 * 获取空气质量指数的aqi
 * @author zhoujianjun
 * http://www.tuling123.com/openapi/cloud/act.jsp  图灵机器人
 */


class AqiCommand extends CConsoleCommand
{
	
	/**
	 * 有PM25数据的城市  152个城市
	 * @var unknown
	 */
	//public $citys = array("三亚","三门峡","上海","东莞","东营","中山","临安","临汾","临沂","丹东","丽水","义乌","乌鲁木齐","九江","乳山","云浮","佛山","保定","克拉玛依","兰州","包头","北京","北海","南京","南充","南宁","南昌","南通","即墨","厦门","句容","台州","合肥","吉林","吴江","呼和浩特","咸阳","哈尔滨","唐山","嘉兴","嘉峪关","大同","大庆","大连","天津","太仓","太原","威海","宁波","安阳","宜兴","宜宾","宜昌","宝鸡","宿迁","富阳","寿光","岳阳","常州","常德","常熟","平度","平顶山","广州","库尔勒","廊坊","延安","开封","张家口","张家港","张家界","徐州","德州","德阳","惠州","成都","扬州","承德","抚顺","拉萨","招远","揭阳","攀枝花","文登","无锡","日照","昆山","昆明","曲靖","本溪","杭州","枣庄","柳州","株洲","桂林","梅州","武汉","汕头","汕尾","江门","江阴","沈阳","沧州","河源","泉州","泰安","泰州","泸州","洛阳","济南","济宁","海口","海门","淄博","淮安","深圳","清远","温州","渭南","湖州","湘潭","湛江","溧阳","滨州","潍坊","潮州","烟台","焦作","牡丹江","玉溪","珠海","瓦房店","盐城","盘锦","石嘴山","石家庄","福州","秦皇岛","章丘","绍兴","绵阳","聊城","肇庆","胶南","胶州","自贡","舟山","芜湖","苏州","茂名","荆州","荣成","莱州","莱芜","莱西","菏泽","营口","葫芦岛","蓬莱","衡水","衢州","西宁","西安","诸暨","贵阳","赤峰","连云港","遵义","邢台","邯郸","郑州","鄂尔多斯","重庆","金华","金坛","金昌","铜川","银川","锦州","镇江","长春","长沙","长治","阳江","阳泉","青岛","鞍山","韶关","马鞍山","齐齐哈尔");
	
	public $citys = array("三亚","三门峡","上海","东莞","东营","中山","临汾","临沂","丹东","丽水","乌鲁木齐","九江","佛山","保定","克拉玛依","兰州","包头","北京","北海","南京","南充","南宁","南昌","南通","厦门","台州","合肥","吉林","呼和浩特","咸阳","哈尔滨","唐山","嘉兴","大同","大庆","大连","天津","太原","威海","宁波","安阳","宜宾","宜昌","宝鸡","宿迁","岳阳","常州","常德","平顶山","广州","廊坊","延安","开封","张家口","张家界","徐州","德州","德阳","惠州","成都","扬州","承德","抚顺","拉萨","攀枝花","无锡","日照","昆明","曲靖","本溪","杭州","枣庄","柳州","株洲","桂林","武汉","汕头","江门","沈阳","沧州","河源","泉州","泰安","泰州","泸州","洛阳","济南","济宁","海口","淄博","淮安","深圳","清远","温州","渭南","湖州","湘潭","湛江","滨州","潍坊","烟台","焦作","牡丹江","玉溪","珠海","盐城","盘锦","石嘴山","石家庄","福州","秦皇岛","绍兴","绵阳","聊城","肇庆","自贡","舟山","芜湖","苏州","荆州","莱芜","菏泽","营口","葫芦岛","衡水","衢州","西宁","西安","贵阳","赤峰","连云港","遵义","邢台","邯郸","郑州","鄂尔多斯","重庆","金华","金昌","铜川","银川","锦州","镇江","长春","长沙","长治","阳泉","青岛","鞍山","韶关","马鞍山","齐齐哈尔");
	
	
	/**
	 * 获取一个城市的全部检测点的PM25值
	 * @var unknown
	 */
	protected $url = 'http://www.tuling123.com/openapi/api';
	
	/**
	 * token
	 * @var unknown
	 */
	protected $apiKey = 'f5f3455926f495bbaa1d14eb1c9f6402';
	
	// 不通过这个接口获取PM25的城市
	protected $excludeCity = array('北京');
	
	
	/**
	 * (non-PHPdoc)
	 * @see CConsoleCommand::init()
	 */
	public function init()
	{
		set_time_limit(0);
	}
	
	
	/**
	 * 初始化城市,只执行一次
	 */
	public function actionOne()
	{
		return true;
		
		foreach ($this->citys as $city)
		{
			$model = new AqiIndex();
			$model->attributes = array(
				'city' => $city,
				'name' => $city,
				'aqi'  => rand(5, 200),
				'update_time' => time(),
				'time_point' => '2014-09-11:15'
			);
			$model->save();
		}
		
	}
	
	/**
	 * 获取空气质量指数
	 * 
	 * php yiic.php aqi begin
	 */
	public function actionBegin()
	{
		$tpl = $this->url . '?key=' . $this->apiKey . '&info=%s空气质量';
		$today = strtotime(date('Y-m-d'));
		
		foreach ($this->citys as $city)
		{
			
			$aqi = NULL;
			
// 			if ($city == '北京')
// 			{
// 				$content = file_get_contents('http://www.chapm25.com/city/beijing.html');
// 				if (preg_match('/.*<span\s+class="popoverchart\s+ppbottom"\s+cityid="1"\s*>(.*)<img\s.*src=".*"><\/span>.*/', $content, $match))
// 				{
// 					$aqi = trim($match[1]);
// 					$time_point = date('Y-m-d:H');
// 				}
// 			}
// 			else
// 			{
				$url = sprintf($tpl, $city);
				$result = json_decode(file_get_contents($url), true);
				if ($result && $result['code'] == '100000')
				{
					$aqi = intval(mb_substr($result['text'], 0, 3));
					$m = preg_match('/.*(\d{2})月(\d{2})日(\d{2})时.*/', $result['text'], $match);
					if ($m)
					{
						$time_point = date('Y') . '-' . $match[1] . '-' . $match[2] . ':' . $match[3];
					}
					else
						$time_point = '0';
				
				}
		//	}
			
			if (($aqi !== NULL) && (intval($aqi)>0))
			{
				$history = new AqiHistory();
				$history->attributes = array(
					'city' => $city,
					'name' => $city,
					'aqi' => $aqi,
					'time_point' => $time_point,
					'date' => $today
				);
				$history->save();
				
				// AQI 最新的
				$current = AqiIndex::model()->findByAttributes(array('city'=>$city));
				if (!empty($current))
				{
					$current->attributes = array(
						'aqi' => $aqi,
						'update_time' => time(),
						'time_point' => $history->time_point
					);
					$current->save();
				}
				
			}
			
		}
	}
	
}