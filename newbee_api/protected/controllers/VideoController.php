<?php

/**
 * 处理CC视频回调
 * @author zhoujianjun
 *
 */
class VideoController extends CController
{
	
	
	/**
	 * 接受回调  http://112.126.73.73/video/notify/qrcode/{qrcode}
	 */
	public function actionNotify()
	{
		Yii::log(print_r($_GET, true), 'info');
		
		$request = Yii::app()->request;
		$qrcode = $request->getParam('qrcode'); // 净化器二维码
		$status = $request->getParam('status');
		$video_id = $request->getParam('videoid'); // 视频id
		$duration = intval($request->getParam('duration'));  //视频时长
		$image = $request->getParam('image'); // 视频封面
		
		$model = CleanerVideo::model()->findByPk($qrcode);
		if (empty($model))
			$model = new CleanerVideo();
		
		$model->attributes = array(
			'qrcode' => $qrcode,
			'video_id' => $video_id,
			'status' => $status == 'OK' ? 1 : 0,
			'duration' => $duration,
			'image' => $image,
			'add_time' => time()
		);
		if (!$model->save())
			Yii::log(print_r($model->getErrors(), true));
			//print_r($model->getErrors());
		else 
		{
				//ob_clean();
				$content = <<<OT
<?xml version="1.0" encoding="UTF-8"?>
<result>OK</result>
OT;
			header('Content-Type:text/xml');
			echo $content;
			die;
		}
		
	}
}