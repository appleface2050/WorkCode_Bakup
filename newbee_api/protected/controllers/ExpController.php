<?php

class ExpController extends CController
{
	/**
	 * 导出
     * 数据一：所有的净化器 任意滤芯寿命小于20%的 净化器  包含字段：序列号 用户昵称，手机号，注册时间，所在地
	 */
	public function actionData1()
	{
		set_time_limit(0);
        header("Content-Type: application/vnd.ms-excel; charset=utf-8");
        header("Content-Disposition: attachment; filename=cleaner".date('H-i-s').".xls");
        header("Pragma: no-cache");
        header("Expires: 0");
        self::echoExcel(array('序列号','昵称','手机号','注册时间','所在城市'));

		//$filepath = Yii::getPathOfAlias('webroot') . '/'.date('H-i-s').'.txt';
		$condition = array(
			'select' => 'id,qrcode,city,filter_surplus_life,type,version',
			//'limit' => 1000
		);
		$result = CleanerStatus::model()->findAll($condition);
		foreach($result as $val)
		{
            if(!$val->filter_surplus_life)
                continue;
            $lifes = CleanerStatus::countSurplusLife($val->filter_surplus_life,$val->type,$val->version);
			if(!$lifes)
            {
                continue;
            }
            $notice = false;
            foreach($lifes as $life)
            {
                if($life['life_value'] < 20)
                {
                    $notice = true;
                    break;
                }
            }
            if(!$notice)
                continue;
            //获取关联的用户信息
            $users = UserCleaner::model()->findAllByAttributes(array('cleaner_id' => $val->id));
            if($users)
            {
                foreach($users as $user)
                {
                    $mobile = $user['user']['mobile'];
                    $nickname = $user['user']['nickname'];
                    self::echoExcel(array(
                            $val->qrcode.'#',
                            $nickname ? $nickname : '无',
                            $mobile ? $mobile : '无',
                            self::getTime($user['user']['add_time']),
                            $val->city
                    ));
                }
            }
            unset($lifes,$life,$users,$user);
		}
	}


    //数据二： 高大卫士，01 开头的序列号净化器
    // 字段：序列号，首次使用时间，最后通讯时间，滤芯寿命 1,2,3 ， 硬件版本号
    public function actionData2()
    {
        set_time_limit(0);
        header("Content-Type: application/vnd.ms-excel; charset=utf-8");
        header("Content-Disposition: attachment; filename=cleaner".date('H-i-s').".xls");
        header("Pragma: no-cache");
        header("Expires: 0");
        self::echoExcel(array('序列号','净化器id','首次使用时间','最后通讯时间','硬件版本号','滤芯寿命'));

        $condition = array(
            'select' => 'id,qrcode,first_use_time,update_time,filter_surplus_life,type,version',
            'condition' => 'type = 1',
            'order' => 'first_use_time ASC',
            //'limit' => 20
        );
        $result = CleanerStatus::model()->findAll($condition);
        foreach($result as $val)
        {
            $life_str = $val->filter_surplus_life;
           /* if($val->filter_surplus_life)
            {
                $lifes = CleanerStatus::countSurplusLife($val->filter_surplus_life,$val->type,$val->version);
                foreach($lifes as $life)
                {
                    $life_str.= $life['id'].':'.$life['surplus_life']."======";
                }
            }*/
            self::echoExcel(
                array(
                    $val->qrcode.'#',
                    $val->id,
                    self::getTime($val->first_use_time),
                    self::getTime($val->update_time),
                    $val->version,
                    $life_str,
                )
            );

            unset($lifes);
        }
    }


    static function getTime($time)
    {
        return $time ? date('Y-m-d H:i:s',$time) : '未知';
    }


    static function echoExcel($titleArr)
    {
        $str='';
        foreach ($titleArr as $title)
        {
            $str.=iconv("utf-8", "GBK//IGNORE", $title)."\t";
        }
        $str = trim($str,'\t');
        echo $str."\n";
    }


    public function actionUser()
    {
        set_time_limit(0);
        header("Content-Type: application/vnd.ms-excel; charset=utf-8");
        header("Content-Disposition: attachment; filename=cleaner".date('H-i-s').".xls");
        header("Pragma: no-cache");
        header("Expires: 0");
        self::echoExcel(array('昵称','手机号','app版本号','和注册时间'));

        $result = User::model()->findAll();
        foreach($result as $val)
        {
           self::echoExcel(
                array(
                    $val->nickname,
                    $val->mobile,
                    $val->app_version,
                    self::getTime($val->add_time)
                )
            );
        }
        //echo 'ok';
    }

}