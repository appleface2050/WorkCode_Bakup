<?php


/**
 * 预定
 * @author zhoujianjun
 *
 */
class ReserveController extends CController
{
	
	
	/**
	 * 预定
	 */
	public function actionCreate()
	{
		$value = htmlspecialchars(Yii::app()->request->getParam('value'));
		$date = date("Y-m-d");
		//$sql = "INSERT INTO reserve(value) VALUES('" . $value ."')";
		$sql = "INSERT INTO reserve(value,add_time,date) VALUES('" . $value . "',".time().",'" . $date . "')";
		Yii::app()->db->createCommand($sql)->execute();
		$callback = htmlspecialchars(Yii::app()->request->getParam('callback'));
		echo "{$callback}({\"msg\":\"成功\"})";
		exit();
	}
	
	
	/**
	 * 
	 */
	public function actionControl()
	{
		$all = Yii::app()->redis->sMembers('ALLONLINE');
		$total = 0;
		foreach ($all as $cleaner_id)
		{
			$cleaner = CleanerStatus::model()->findByPk($cleaner_id);
			if (!empty($cleaner))
			{
				$timeset = json_decode($cleaner->timeset, true);
				if (!empty($timeset))
					$total++;
				$set = CleanerStatus::model()->getFormatTimeset($cleaner);
				
				$key = EKeys::getCleanerCacheKey($cleaner->id);
				
				$message = $cleaner->id . '-' . CleanerStatus::OP_CODE_TIMESET;

				Yii::app()->redis->hSet($key, 'timeset', join('#', $set));
				
				
				//发送指令
				Yii::app()->publish->publishParameter($message);
			}
		}
		
	}
	
}
