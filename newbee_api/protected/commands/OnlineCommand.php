<?php

/**
 * 各种在线数量的统计
 * @author zhoujianjun
 *
 */
class OnlineCommand extends CConsoleCommand
{
	
	/**
	 * (non-PHPdoc)
	 * @see CConsoleCommand::init()
	 */
	public function init()
	{
		set_time_limit(0);
		Yii::import('application.modules.admin.models.ActiveUserHistory');
		Yii::import('application.modules.admin.models.ActiveUserCount');
		Yii::import('application.modules.admin.models.OnlineCleanerCount');
		Yii::import('application.modules.admin.models.OnlineCleanerHistory');
		error_reporting(0);
	}
	
	
	/**
	 * 只一次
	 */
	public function actionInit()
	{
		$keys = array(
			'2015-01-01' => 'HISTORYLINE-2015-1-1', 
			'2015-01-02' => 'HISTORYLINE-2015-1-2',
			'2015-01-03' => 'HISTORYLINE-2015-1-3', 
			'2015-01-04' => 'HISTORYLINE-2015-1-4',
			'2015-01-05' => 'HISTORYLINE-2015-1-5',
			'2015-01-06' => 'HISTORYLINE-2015-1-6',
			'2015-01-07' => 'HISTORYLINE-2015-1-7',
			'2015-01-08' => 'HISTORYLINE-2015-1-8',
			'2015-01-09' => 'HISTORYLINE-2015-1-9',
			'2015-01-10' => 'HISTORYLINE-2015-1-10',
			'2015-01-11' => 'HISTORYLINE-2015-1-11',
			'2015-01-12' => 'HISTORYLINE-2015-1-12',
			'2015-01-13' => 'HISTORYLINE-2015-1-13',
			'2015-01-14' => 'HISTORYLINE-2015-1-14',
		);
		
		foreach ($keys as $day => $key)
		{
			$all = Yii::app()->redis->sMembers($key);
			$data_char = $day;
			$data = strtotime($data_char);
			$time = time();
			
			if (!empty($all))
			{
				$user_ids = array_chunk($all, 100);
					
				foreach ($user_ids as $value)
				{
					$sql = "insert into " . OnlineCleanerHistory::model()->tableName() . "(date,cleaner_id,add_time,date_char) values ";
					foreach ($value as $user_id)
					{
						$sql .= "(" . $data . ",'" . $user_id . "'," . $time . "," . "'" . $data_char . "'),";
					}
					$sql = rtrim($sql, ',');
					Yii::app()->db->createCommand($sql)->execute();
				}
			}
			// 数量统计
			$total = count($all);
			//$sql = "insert into " . OnlineCleanerCount::model()->tableName() . "(date,total,add_time,date_char) values(" . $data . "," . $total . "," . $time . ",'" . $data_char . "')";
			$sql = "update " . OnlineCleanerCount::model()->tableName() . " set total=" . $total . " where date_char='" . $data_char . "'";
			
			Yii::app()->db->createCommand($sql)->execute();
		}
		
	}
	
	/**
	 * 起始
	 */
	public function actionBegin()
	{
		$this->actionCleaner();
		$this->actionUser();
	}
	
	/**
	 * 用户在线统计
	 */
	public function actionUser()
	{
		// 昨天活跃的用户的集合
		$key = EKeys::getActiveUserKey(true);
		$all = Yii::app()->redis->sMembers($key);
		$data_char = date('Y-m-d', strtotime("-1 day"));
		$data = strtotime($data_char);
		$time = time();
		
		if (!empty($all))
		{
			$user_ids = array_chunk($all, 100);
			
			foreach ($user_ids as $value)
			{
				$sql = "insert into " . ActiveUserHistory::model()->tableName() . "(date,user_id,add_time,date_char) values ";
				foreach ($value as $user_id)
				{
					$sql .= "(" . $data . "," . $user_id . "," . $time . "," . "'" . $data_char . "'),";
				}
				$sql = rtrim($sql, ',');
				Yii::app()->db->createCommand($sql)->execute();
			}
		}
		// 数量统计
		$total = count($all);
		$sql = "insert into " . ActiveUserCount::model()->tableName() . "(date,total,add_time,date_char) values(" . $data . "," . $total . "," . $time . ",'" . $data_char . "')";
		Yii::app()->db->createCommand($sql)->execute();
		//Yii::app()->redis->delete($key);
	}
	
	/**
	 * 净化器在线统计
	 */
	public function actionCleaner()
	{
		$key = EKeys::getHistoryKey(true);
		$all = Yii::app()->redis->sMembers($key);
		$data_char = date('Y-m-d', strtotime("-1 day"));
		$data = strtotime($data_char);
		$time = time();
		
		if (!empty($all))
		{
			$cleaner_ids = array_chunk($all, 100);
			
			foreach ($cleaner_ids as $value)
			{
				$sql = "insert into " . OnlineCleanerHistory::model()->tableName() . "(date,cleaner_id,add_time,date_char) values ";
				foreach ($value as $cleaner_id)
				{
					$sql .= "(" . $data . ",'" . $cleaner_id . "'," . $time . "," . "'" . $data_char . "'),";
				}
				$sql = rtrim($sql, ',');
				Yii::app()->db->createCommand($sql)->execute();
			}
		}
		
		$total = count($all);
		$sql = "insert into " . OnlineCleanerCount::model()->tableName() . "(date,total,add_time,date_char) values(" . $data . "," . $total . "," . $time . ",'" . $data_char . "')";
		Yii::app()->db->createCommand($sql)->execute();
		
		// @todo  删除统计
		//Yii::app()->redis->delete($key);
		
	}
	
	
}