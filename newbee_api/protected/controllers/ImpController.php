<?php

class ImpController extends CController
{
	
	
	
	/**
	 * 导入
	 */
	public function actionImport()
	{
		set_time_limit(0);
		
		$filepath = Yii::getPathOfAlias('webroot') . '/new.txt';
		
		$data = file($filepath);
		
		$release_date = date('Y-m-d');
		
		$version = '3.0.1';
		
		$time = time();
		
		$type = 6;

        $success_count = 0;
		foreach ($data as $value)
		{
            if(!$value)
                continue;
			$cleaner = new Cleaner();
			$cleaner->attributes = array(
				'qrcode' => trim($value),
				'cleaner_id' => '',
				'release_date' => $release_date,
				'version' => $version,
				'add_time' => time(),
				'type' => $type
			);
			if (!$cleaner->save())
			{
				print_r($cleaner->getErrors());
			}else
            {
                $success_count++;
            }
		}
        echo 'ok:'.$success_count;
	}
	
	public function actionSn()
	{
		$filepath = Yii::getPathOfAlias('webroot') . '/new.txt';

		$data = file($filepath);
		$sn = ' ';
		foreach($data as $s)
		{
			$s = trim($s);
			$sn.=" '{$s}',";
		}
		$sn =trim($sn,',');

		$arr[] = ' select * from cleaner where qrcode in ('.$sn.')';
		$arr[] = ' update cleaner set cleaner_id = "" where qrcode in ('.$sn.')';
        $arr[] = 'delete from  user_cleaner_rel where cleaner_id in (select id from cleaner_status where qrcode in ('.$sn.'))';
		$arr[] = 'delete from cleaner_status where qrcode in ('.$sn.')';
		echo implode('<hr>',$arr);
	}
}