<?php

/**
 * 分享管理,分享文章活动签名
 * @author zhoujianjun
 *
 */
class ShareController extends RestController
{
	
	/**
	 * 获取分享的数据
	 */
	public function actionData()
	{
		$data = UserrankCurrent::model()->detail(Yii::app()->user->id);
		$data['img'] = $this->drawShareImg($data);
		$this->success($data);
	}
	
	/**
	 * 分享成功后的回调函数,统计分享次数
	 * @param string token 用户token
	 * @param string 净化器id
	 * @param string platform 分享平台
	 */
	public function actionCallback()
	{
		$platform = intval(Yii::app()->request->getParam('platform'));
		$share = new UserShare();
		$share->attributes = array(
			'user_id' => Yii::app()->user->id,
			'platform' => $platform,
		);
		$share->save();
		$data = UserShare::model()->getNotice(Yii::app()->user->id);
		$this->success($data);
	}
	
	
	
	/**
	 * 分享的图片
	 * @param unknown $data
	 */
	private function drawShareImg($data)
	{
		//
		$text1 = $data['city'] . 'PM2.5全天平均' . $data['outer_pm25_index'] . '，我';
		$text2 = '家室内全天平均' . $data['inner_pm25_index'] . '，比全国';
		$text3 =  $data['pm25_ratio'] . '的家庭更洁净。';
		
		
		$text4 = '空气净化总量' . $data['total_clean_volume'] . '立方米，超';
		$text5 = '过'. $data['volumn_ratio'] .'的家庭';
		
		
		$image = imagecreatetruecolor(200, 200);
		$transparent = imagecolorallocatealpha($image, 255, 255, 255, 127);
		imagefill($image, 0, 0, $transparent);
		
		// Replace path by your own font path
		$font  =  './msyh.ttf' ;
		
		$black  =  imagecolorallocate ($image ,  50 , 50 ,  50 );
		$fontSize = 10;
		
		// Add the text
		imagettftext ($image ,  $fontSize ,  0 ,  5 ,  30 ,  -$black ,  $font ,  $text1);
		imagettftext ($image ,  $fontSize ,  0 ,  5 ,  55 ,  -$black ,  $font ,  $text2);
		imagettftext ($image ,  $fontSize ,  0 ,  5 ,  80 ,  -$black ,  $font ,  $text3);
		imagettftext ($image ,  $fontSize ,  0 ,  5 ,  120 ,  -$black ,  $font ,  $text4);
		imagettftext ($image ,  $fontSize ,  0 ,  5 ,  145 ,  -$black ,  $font ,  $text5);
		
		// 输出图像
		//header ( "Content-type: image/png" );
		$filepath = '/images/share/' . Yii::app()->user->id . '.png';
		$path = Yii::getPathOfAlias('webroot') . $filepath;
		imagepng($image, $path);
		return Yii::app()->getRequest()->getHostInfo() . $filepath;
	}


	/**
	 * 滤芯兑换 随机获取分享文本
	 */
	public function actionGetShareText()
	{
		$counts = ShareText::model()->count();
		//var_dump($counts);die;
		if(!$counts)
			$this->failed('没有要分享的内容');
		//随机获取分享数据
		$i = 1;
		while(true)
		{
			if($i>3)
				break;
			$sql = " SELECT *
				FROM `share_text` AS t1 JOIN (SELECT ROUND(RAND() * ((SELECT MAX(id) FROM `share_text`)-(SELECT MIN(id) FROM `share_text`))+(SELECT MIN(id) FROM `share_text`)) AS id) AS t2
				WHERE t1.id >= t2.id
				ORDER BY t1.id LIMIT 1";
			$share = Yii::app()->db->createCommand($sql)->queryRow();
			if($share)
				break;
			$i++;
		}
		unset($share['create_time']);
		if(!$share['image_path'])
			$share['image_path'] = Yii::app()->getRequest()->getHostInfo().'/change_filter/share_default.jpg';
		else
			$share['image_path'] = AttachHelper::getFileUrl($share['image_path']);
		$this->success($share);
	}

}