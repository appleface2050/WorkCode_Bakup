<?php

/**
 * 统计净化器每天净化量和每个用户多个净化器pm25平均值和净化总量
 * @author zhoujianjun
 *
 */
class StatCommand extends CConsoleCommand
{
	
	
	/**
	 * (non-PHPdoc)
	 * @see CConsoleCommand::init()
	 */
	public function init()
	{
		set_time_limit(0);
		//date_default_timezone_set('PRC');
	}
	
	
	/**
	 * crontab 调用, 将2个统计合并调用
	 * php yiic.php stat start
	 */
	public function actionStart()
	{
		$this->statCleanerDay();
		$this->statUserDay();
	}
	
	/**
	 * 净化器每天统计pm25平均值和总净化量
	 * php yiic.php stat cleanerDay
	 */
	public function actionCleanerDay()
	{
		$this->statCleanerDay();
	}
	
	/**
	 * 统计用户全部的净化器净化总量及pm25均值(按家庭计算)
	 * php yiic.php stat userday
	 */
	public function actionUserDay()
	{
		$this->statUserDay();
	}
	
	/**
	 * 净化器每天统计pm25平均值和总净化量
	 */
	private function statCleanerDay()
	{
		Yii::log('=================================================','info');
		Yii::log('statCleanerDay===start','info');
		$sql = "SELECT count(id) AS total FROM cleaner_status";
		$total = Yii::app()->db->createCommand($sql)->queryScalar();
		$init_model = new StatCleanerDay();
		// 昨天的Y-m-d时间戳
		$yesterday = strtotime(date('Y-m-d', strtotime('-1 day')));
		if ($total > 0)
		{
			// 100条记录每次
			$n = 100; 
			$j = ceil($total/100);
			for ($i=0; $i<$j; $i++)
			{
				// 每次100条
				$sql = "SELECT id FROM cleaner_status LIMIT ".$i*$n.",".$n;
				$result = Yii::app()->db->createCommand($sql)->queryAll();
				foreach ($result as $value)
				{
					// 昨天全天该净化器的PM值(可能均值,也可能是其余的算法)
					$cleaner_id = $value['id'];
					$average = $this->getCleanerPm25($cleaner_id, $yesterday);
					// 昨天全天该净化器的空气净化总量
					$totalVolumn = $this->getCleanerVolumn($cleaner_id, $yesterday);
					
					$model = clone $init_model;
					$model->attributes = array(
						'cleaner_id' => $cleaner_id,
						'pm25_index' => $average,
						'clean_volumn' => $totalVolumn,
						'date' => $yesterday
					);
					$model->save();
				}
				$result = null;
			}
		}
		Yii::log('statCleanerDay===finish','info');
		echo 'statCleanerDay===finish'.PHP_EOL;
	}
	
	
	/**
	 * 统计用户全部的净化器净化总量及pm25均值(按家庭计算)
	 */
	private function statUserDay()
	{
		Yii::log('statUserDay===start','info');

		$sql = "SELECT count(id) AS total FROM user";
		$total = Yii::app()->db->createCommand($sql)->queryScalar();
		
		// 外部pm25	城市=>pm25值   的映射
		$map = array();
		
		// 昨天的Y-m-d时间戳
		$yesterday = strtotime(date('Y-m-d', strtotime('-1 day')));

		$rank_model = new UserrankDay();
		if ($total > 0)
		{
			// 100条记录每次
			$n = 50;
			$j = ceil($total/$n);
			for ($i=0; $i<$j; $i++)
			{
				Yii::app()->db->setActive(true);
				$sql = "SELECT id FROM user LIMIT ".$i*$n.",".$n;
				$result = Yii::app()->db->createCommand($sql)->queryAll();
				foreach ($result as $value)
				{
					$sql = "SELECT ucr.user_id,city,AVG(pm25_index) as pm25,SUM(clean_volumn) AS clean_volumn FROM user_cleaner_rel as ucr, stat_cleaner_day as scd WHERE ucr.user_id={$value['id']} and ucr.cleaner_id=scd.cleaner_id AND scd.date={$yesterday} GROUP BY ucr.user_id";

					$row = Yii::app()->db->createCommand($sql)->queryRow();
					if (!empty($row))
					{
						$city = $row['city'];
						if (!isset($map[$city]))
						{
							$outer_avg = Yii::app()->location->getAvagePm($city, $yesterday);
							$map[$city] = $outer_avg;
						}
							
						$model = clone $rank_model;
						$model->attributes = array(
							'user_id' => $value['id'],
							'total_clean_volume' => $row['clean_volumn'],
							'inner_pm25_index' => floor($row['pm25']),
							'city' => $city,
							'outer_pm25_index' => $map[$city],
							'date' => $yesterday,
							'add_time' => time()
						);
						$model->save();
						// 更新缓存
						$sql = "INSERT INTO stat_userrank_current(user_id,total_clean_volume,inner_pm25_index,outer_pm25_index,city,add_time) VALUES(".
								$value['id'] . "," . $row['clean_volumn'] . "," . $row['pm25'] . "," . $map[$city] . ",'" . $city . "'," . time() . ") ON DUPLICATE KEY UPDATE total_clean_volume=".
								$row['clean_volumn'] . ",inner_pm25_index=" . $row['pm25'] . ",outer_pm25_index=" . $map[$city] . ",city='" . $city . "',add_time=" . time();
						
						Yii::app()->db->createCommand($sql)->execute();
					}
				}
				$result = null;
				echo '===sleep'.PHP_EOL;
				sleep(2);
			}
			
			// 计算比率
			$sql = "SELECT * FROM stat_userrank_day WHERE date={$yesterday}";
			$result = Yii::app()->db->createCommand($sql)->queryAll();
			if (!empty($result))
			{
				$volumn = array();
				$pm = array();
				foreach ($result as $key => $value)
				{
					$volumn[$key] = $value['total_clean_volume'];
					$pm[$key] = $value['inner_pm25_index'];
				}
				
				// 总数
				$total = count($result);
				$data = $result;
				
				// 净化量排名比率
				array_multisort($volumn, SORT_DESC, $data);
				$index = 1;
				$beforeVolumn = null;
				
				foreach ($data as $key => $value)
				{
					if ($key >0 && ($value['total_clean_volume'] == $beforeVolumn))
					{
						
						
					}
					else 
					{
						$index = $key + 1;
					}
					$beforeVolumn = $value['total_clean_volume'];
					
					$ratio = round(1 - $index/$total, 2) * 100;
					$sql = "update stat_userrank_day set volumn_ratio='".$ratio."' where id=".$value['id'];
					
					Yii::app()->db->createCommand($sql)->execute();
					
					// 更新stat_userrank_current表
					$sql = "update stat_userrank_current set volumn_ratio='".$ratio."' where user_id=" . $value['user_id'];
					Yii::app()->db->createCommand($sql)->execute();
				}
				
				// pm25的比率
				array_multisort($pm, SORT_ASC, $result);
				$index = 1;
				$beforePm25 = null;
				foreach ($result as $key => $value)
				{
					if ($key >0 && ($value['inner_pm25_index'] == $beforePm25))
					{
					
					
					}
					else
					{
						$index = $key + 1;
					}
					
					$beforePm25 = $value['inner_pm25_index'];
					$ratio = round(1 - $index/$total, 2) * 100;
					$sql = "update stat_userrank_day set pm25_ratio='".$ratio."' where id=".$value['id'];
					Yii::app()->db->createCommand($sql)->execute();
					
					$sql = "update stat_userrank_current set pm25_ratio='".$ratio."' where user_id=".$value['user_id'];
					Yii::app()->db->createCommand($sql)->execute();
				}
			}
		}
		Yii::log('statUserDay===finish','info');
		echo 'statUserDay===finish';
		
	}
	
	/**
	 * 获取昨天(哪一天)1台空气净化器的全天PM25均值, 目前取的是均值
	 * @param $cleaner_id 净化器id
	 * @param $datetime 日期时间戳
	 * @todo 可能用其它算法求全天的PM25值(均值容易受极值的影响)
	 * ALTER TABLE `pm25_used_history` ADD INDEX (`cleaner_id`, `date`);
	 */
	private function getCleanerPm25($cleaner_id, $datetime)
	{
		$sql = "SELECT AVG(value) AS average FROM pm25_used_history WHERE cleaner_id='{$cleaner_id}' AND date={$datetime}";
		//echo $sql.PHP_EOL;
		$average = Yii::app()->db->createCommand($sql)->queryScalar();
		return empty($average) ? 0 : floor($average);
	}
	
	
	/**
	 * 获取昨天一台净化器的空气净化总量
	 * @param unknown $cleaner_id
	 * @param unknown $yesterday
	 */
	private function getCleanerVolumn($cleaner_id, $datetime)
	{
		$total = 0;
		
		// 在各个档位下的净化时间(秒为单位)
		$map = array(
			CleanerStatus::LEVEL_SILENCE => 0,
			CleanerStatus::LEVEL_OEN => 0,
			CleanerStatus::LEVEL_TWO => 0,
			CleanerStatus::LEVEL_THREE => 0,
			CleanerStatus::LEVEL_FOUR => 0
		);
		
		$sql = "SELECT * FROM level_used_history WHERE cleaner_id='{$cleaner_id}' AND date={$datetime} AND level !=" . CleanerStatus::LEVEL_SLEEP;
		$result = Yii::app()->db->createCommand($sql)->queryAll();
		
		if (!empty($result))
		{
			$first = array_shift($result);
			
			// 起始level
			$level = $first['level'];
			// 起始时间
			$startTime = $first['add_time'];
			
			foreach ($result as $value)
			{
				$map[$level] += $value['add_time'] - $startTime;
				$level = $value['level'];
				$startTime = $value['add_time'];
			}
			
			// @tip 最后一个档位的运行时间没有计算进去
			foreach ($map as $level => $time)
			{
				$total += number_format($time / 3600 * CleanerStatus::$cleanVolumnMap[$level], 2);
			}
		}
		return floor($total);
	}
	
	/**
	 * 一些特殊处理的
	 */
	public function actionSpecial()
	{
		$cleaner_id = '60C5A8604C73';
		$yesterday = strtotime(date('Y-m-d', strtotime('-1 day')));
		
		
		$total = $this->getCleanerVolumn($cleaner_id, $yesterday);
		
		echo 'Over:' . $total;
	}
	
}