<?php

/**
 * 预处理相关
 * @author zhoujianjun
 *
 */
class PrepareCommand extends CConsoleCommand
{
	
	public function init()
	{
		set_time_limit(0);
	}
	
	/**
	 * php yiic.php prepare filterlife
	 * 首先清除表里container_filterlife_today中的数据
	 * 然后将cleaner_status表里净化器滤芯寿命小于15%的净化器的id记录在表container_filterlife_today中
	 * 凌晨执行
	 */
	public function actionFilterlife()
	{
		// 清除表container_filterlife_today中的数据(昨天滤芯寿命小于15%的净化器的id)
		$sql = "truncate table container_filterlife_today";
		Yii::app()->db->createCommand($sql)->execute();
		
		// 查找符合条件的滤芯的id
		$sql = "SELECT count(id) AS total FROM cleaner_status";
		$total = Yii::app()->db->createCommand($sql)->queryScalar();
		if ($total>0)
		{
			// 100条记录每次
			$n = 100;
			$j = ceil($total/100);
			for ($i=0; $i<$j; $i++)
			{
				$sql = "SELECT id,filter_surplus_life,type from cleaner_status LIMIT ".$i*$n.",".$n;
				$result = Yii::app()->db->createCommand($sql)->queryAll();
				if (!empty($result))
				{
					foreach ($result as $value)
					{
						$life = json_decode($value['filter_surplus_life'], true);
						if (!empty($life))
						{
							$defaultLife = CleanerStatus::getDefaultLife($value['type']);
							$i = 1;
							$exist = false;
							foreach ($life as $id => $surplus)
							{
								if ($value['type'] == CleanerStatus::TYPE_BIG_WITH)
									$id = $i;
								if (round($surplus/$defaultLife[$id],2) < 0.15)
								{
									// 少于15%, 只要有一个满足条件 就记录
									$exist = true;
									break;
								}
								$i++;
							}
							
							if ($exist)
							{
								$sql = "insert into container_filterlife_today values('{$value['id']}')";
								Yii::app()->db->createCommand($sql)->execute();
							}
							
						}
					}
				}
			}
		}
		
	}
	
	
}