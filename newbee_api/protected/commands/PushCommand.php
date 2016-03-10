<?php

/**
 * 推送相关的命令
 * @author zhoujianjun
 *
 */
class PushCommand extends CConsoleCommand
{
	
	
	/**
	 * (non-PHPdoc)
	 * @see CConsoleCommand::init()
	 */
	public function init()
	{
		set_time_limit(0);
	}
	
	
	/**
	 * 昨日净化排名  消息id(infoid 为1, 分享页面)
	 * 内容模板: 昨天您家比全国[百分比]的家庭更干净，空气净化总量超过[百分比]的家庭。  @tip  干净指的是pm25
	 * 着陆页面： 昨日您家空气质量排名页
	 * 通知时间： 每天上午通知1次
	 */
	public function actionRank()
	{
		$sql = "SELECT count(user_id) AS total FROM user_token";
		$total = Yii::app()->db->createCommand($sql)->queryScalar();
		
		$contentTpl = "昨天您家比全国%s%%的家庭更干净，空气净化总量超过%s%%的家庭。";
		if ($total > 0)
		{
			// 100条记录每次
			$n = 100; 
			$j = ceil($total/100);
			for ($i=0; $i<$j; $i++)
			{
				$sql = "SELECT suc.* FROM stat_userrank_current AS suc,user_token AS token WHERE suc.user_id=token.user_id LIMIT " . $i*$n.",".$n;
				$result = Yii::app()->db->createCommand($sql)->queryAll();
				if (!empty($result))
				{
					foreach ($result as $value)
					{
						$data = array(
							'title'   => '三个爸爸',
							'content' => sprintf($contentTpl, $value['pm25_ratio'], $value['volumn_ratio']),
							'info_id'  => 1
						);
						Yii::app()->push->push($value['user_id'], $data);
					}
				}
			}
		}
	}
	
	/**
	 * 滤芯寿命推送
	 * php yiic.php push filter
	 */
	public function actionFilter()
	{
		$sql = "SELECT count(*) AS total FROM container_filterlife_today";
		$total = Yii::app()->db->createCommand($sql)->queryScalar();
		
		if ($total > 0)
		{
			// 100条记录每次
			$n = 100;
			$j = ceil($total/100);
			for ($i=0; $i<$j; $i++)
			{
				$sql = "select cleaner.id,cleaner.filter_surplus_life,cleaner.type from cleaner_status as cleaner,container_filterlife_today as t where t.cleaner_id=cleaner.id limit ".$i*$n.",".$n;
				$result = Yii::app()->db->createCommand($sql)->queryAll();
				foreach ($result as $value)
				{
					$surpluslife = json_decode($value['filter_surplus_life'], true);
					$defaultLife = CleanerStatus::getDefaultLife($value['type']);
					$i = 1;
					$exist = false;
					foreach ($surpluslife as $id => $surplus)
					{
						if ($value['type'] == CleanerStatus::TYPE_BIG_WITH)
							$id = $i;
						
						if (intval($surplus) == 0)
						{
							if (!PushFilterLog::model()->checkPush($value['id'], $id, PushFilterLog::FLAG_OVER))
							{
								// 滤芯寿命为0  首次推送
								$filter_name = CleanerStatus::getDefaultName($value['type'], $id);
								$this->filterOver($value['id'], $filter_name);
								$exist = 1;
								PushFilterLog::model()->pushLog($value['id'], $id, $value['type'], PushFilterLog::FLAG_OVER);
							}
						}
						else 
						{
							if (round($surplus/$defaultLife[$id],2) < 0.10 && (!PushFilterLog::model()->checkPush($value['id'], $id, PushFilterLog::FLAG_SURPLUS)))
							{
								// 滤芯寿命小于10% 首次推送
								$filter_name = CleanerStatus::getDefaultName($value['type'], $id);
								$this->filterLeft($value['id'], $filter_name);
								$exist = 1;
								PushFilterLog::model()->pushLog($value['id'], $id, $value['type'], PushFilterLog::FLAG_SURPLUS);
							}
						}
					}
					$i++;
					break;
				}
			}
		}
	}
	
	/**
	 * 滤芯寿命即将到期
	 * 内容模板: 您的[净化器用户命名,如“卧室”]净化器的[滤芯名称]寿命已低于10%。
	 * 着陆页面： 净化器详细页
	 * 通知时间： 每个滤芯寿命低于10%的时候通知1次（大机器有三个滤芯）
	 * @param $cleaner_id 净化器id
	 * @param $filter_name  滤芯名称
	 */
	public function filterLeft($cleaner_id, $filter_name)
	{
		$contentTpl = '您的%s净化器的滤芯%s寿命已低于10%%，请更换滤芯';
		$result = UserCleaner::model()->with('cleaner')->findAllByAttributes(array('cleaner_id' => $cleaner_id));
		if (!empty($result))
		{
			foreach($result as $value)
			{
				$data = array(
					'title'   => '三个爸爸',
					'content' => sprintf($contentTpl, $value['name'], $filter_name),
					'info_id'  => 2,
					'cleaner_id' => $value['cleaner_id']
				);
				Yii::app()->push->push($value['user_id'], $data);
			}
		}
	}
	
	/**
	 * 滤芯寿命已经到期
	 * 内容模板: 您的[净化器用户命名,如“卧室”]净化器的[滤芯名称]寿命已经为0，请更换滤芯。
	 * 着陆页面： 净化器详细页
	 * 通知时间： 每个滤芯寿命为0时通知1次
	 * @param $cleaner_id 净化器id
	 * @param $filter_name  滤芯名称
	 */
	public function filterOver($cleaner_id, $filter_name)
	{
		$contentTpl = '您的%s净化器的滤芯%s寿命已经为0，请更换滤芯';
		$result = UserCleaner::model()->with('cleaner')->findAllByAttributes(array('cleaner_id' => $cleaner_id));
		if (!empty($result))
		{
			foreach($result as $value)
			{
				$data = array(
					'title'   => '三个爸爸',
					'content' => sprintf($contentTpl, $value['name'], $filter_name),
					'info_id'  => 2,
					'cleaner_id' => $value['cleaner_id']
				);
				Yii::app()->push->push($value['user_id'], $data);
			}
		}
	}
	
}