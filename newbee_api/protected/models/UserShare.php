<?php

/**
 * This is the model class for table "user_share_history".
 *
 * The followings are the available columns in table 'user_share_history':
 * @property string $id
 * @property string $user_id
 * @property integer $platform
 * @property string $cleaner_id
 * @property string $date
 * @property string $add_time
 * @property integer $status
 * 
 * 连续5天分享则获取滤芯
 */
class UserShare extends CActiveRecord
{
	
	/**
	 * 分享平台 1微信好友 2微信朋友圈 3 新浪微博
	 * @var unknown
	 */
	const SHARE_PLATFORM_FRIEND    = 1;
	const SHARE_PLATFORM_QUAN      = 2;
	const SHARE_PLATFORM_SINAWEIBO = 3;
	
	/**
	 * 分享是否已经使用(即是否已用来领取滤芯)1未使用 0已使用
	 * @var unknown
	 */
	const STATUS_NORMAL   = 1;
	const STATUS_CONSUMED = 0;
	
	
	/**
	 * @return string the associated database table name
	 */
	public function tableName()
	{
		return 'user_share_history';
	}

	/**
	 * @return array validation rules for model attributes.
	 */
	public function rules()
	{
		// NOTE: you should only define rules for those attributes that
		// will receive user inputs.
		return array(
			array('add_time', 'default', 'value' => time()),
			array('date', 'default', 'value' => strtotime(date('Y-m-d'))),
			array('user_id, platform, date, add_time', 'required'),
			// The following rule is used by search().
			// @todo Please remove those attributes that should not be searched.
		);
	}

	/**
	 * @return array relational rules.
	 */
	public function relations()
	{
		// NOTE: you may need to adjust the relation name and the related
		// class name for the relations automatically generated below.
		return array(
		);
	}

	/**
	 * @return array customized attribute labels (name=>label)
	 */
	public function attributeLabels()
	{
		return array(
			'id' => 'ID',
			'user_id' => 'User',
			'platform' => 'Platform',
			'date' => 'Date',
			'add_time' => 'Add Time',
			'status' => 'Status',
		);
	}

	/**
	 * Returns the static model of the specified AR class.
	 * Please note that you should have this exact method in all your CActiveRecord descendants!
	 * @param string $className active record class name.
	 * @return UserShare the static model class
	 */
	public static function model($className=__CLASS__)
	{
		return parent::model($className);
	}
	
	/**
	 * 
	 * @param unknown $user_id
	 */
	public function getNotice($user_id)
	{
		// 判断是否已经申请领取过  applied 是否已经申请过滤芯  total 连续分享的次数
		$data = array('applied' => 0, 'enough' => 1, 'is_tip' => 0);
		$sql = "SELECT count(*) AS total FROM user_filter_apply_history WHERE user_id={$user_id}";
		$counts = Yii::app()->db->createCommand($sql)->queryScalar();
		if ($counts >0)
		{
			// 已经分享过了
			$data['applied'] = 1;
			return $data;
		}
		
		$user = User::model()->findByPk($user_id);
		if (intval($user->enough) == 1)
		{
			// 已经够5次分享了
			$data['enough'] = 1;
			$enough_date = $user->enough_date;
			$sql = "SELECT count(*) FROM ".$this->tableName()." WHERE add_time>{$enough_date}";
			$total = Yii::app()->db->createCommand($sql)->queryScalar();
			$data['is_tip'] = $total < 3 ? 1 : 0;
		} else 
		{
			// 未分享5次
			$sql = "SELECT `date` FROM ".$this->tableName()." WHERE user_id={$user_id} GROUP BY `date` ORDER BY `date` DESC ";
			$result = Yii::app()->db->createCommand($sql)->queryColumn();
			$total = count($result);
			if ($total > 5)
			{
				// 分享次数大于5
				$able = true;
				for($i=0; $i<4; $i++)
				{
					// 是否是连着的天
					if ($result[$i] - $result[$i+1] != 86400)
					{
						$able = false;
						break;
					}
				}
				if ($able)
				{
					$data['enough'] = 1;
					User::model()->updateByPk($user_id, array('enough' =>1, 'enough_date'=> strtotime(date('Y-m-d'))));
				}
			}
		}
		return $data;
	}
	
}
