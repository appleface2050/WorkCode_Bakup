<?php

/**
 * 位置相关的
 * @author zhoujianjun
 *
 */
class ELocation extends CApplicationComponent
{
	
	/**
	 * 百度地图获取地址
	 * @var unknown
	 */
	public $url = 'http://api.map.baidu.com/geocoder/v2/?';
	
	/**
	 * 百度地图所用的key
	 * @var unknown
	 */
	const APPKEY = 'ahDQar5r5Y3Ga6Us876FVIs8';
	
	/**
	 * 坐标的类型，目前支持的坐标类型包括：bd09ll（百度经纬度坐标）、gcj02ll（国测局经纬度坐标）、wgs84ll（ GPS经纬度）
	 * @var unknown
	 */
	const COORDTYPE = 'bd09ll';
	
	/**
	 * 默认的城市,及默认返回AQI的城市
	 * @var unknown
	 */
	protected $defaultCity = '北京';
	
	
	/**
	 * 根据经纬度获取所在城市
	 * @param unknown $point_x
	 * @param unknown $point_y
	 * @param string 城市
	 */
	public function getCity($point_x, $point_y)
	{
		$city = '';
		if (empty($point_x) || empty($point_y))
			return $city;
		$param = array(
			'ak'	    => self::APPKEY,
			'coordtype' => self::COORDTYPE,
			'location'  => $point_y . ',' . $point_x,
			'output'    => 'json'
		);
		$url = $this->url . http_build_query($param);
		$result = json_decode(Yii::app()->curl->get($url),true);
		//Yii::log(print_r($result, true), 'info');
		if ($result['status'] == 0) 
		{
			// 成功
			$city = $result['result']['addressComponent']['city'];
			$city = str_replace('市', '', $city);
		}
		return $city;
	}
	
	/**
	 * 根据城市获取pm25,如果该城市没有PM25值,则返回北京的PM25
	 * @param unknown $city
	 */
	public function getPm($city)
	{
// 		if (empty($city))
// 			return '-1';
		
		$city = empty($city) ? $this->defaultCity : $city;
		$sql = "SELECT aqi FROM aqi_outside_index WHERE city='".$city."'";
		$aqi = Yii::app()->db->createCommand($sql)->queryScalar();
		if (empty($aqi))
		{
			//$aqi = '-1';
			$sql = "SELECT aqi FROM aqi_outside_index WHERE city='".$this->defaultCity."'";
			$aqi = Yii::app()->db->createCommand($sql)->queryScalar();
		}
		return intval($aqi);
	}
	
	
	/**
	 * 获取一天平均的PM25值
	 * @param unknown $city
	 * @param int $day  Y-m-d的时间戳
	 */
	public function getAvagePm($city, $day)
	{
		$city = empty($city) ? '北京' : $city;
		$sql = "SELECT AVG(aqi) FROM aqi_outside_history WHERE city='$city' AND date=$day";
		$average = Yii::app()->db->createCommand($sql)->queryScalar();
		return empty($average) ? 0 : floor($average);
	}
}