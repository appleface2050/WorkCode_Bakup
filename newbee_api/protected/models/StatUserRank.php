<?php

/**
 * This is the model class for table "stat_user_rank".
 *
 * The followings are the available columns in table 'stat_user_rank':
 * @property string $id
 * @property string $user_id
 * @property string $total_clean_volume
 * @property double $volumn_ratio
 * @property integer $inner_pm25_index
 * @property double $pm25_ratio
 * @property integer $outer_pm25_index
 * @property string $date
 * @property string $add_time
 */
class StatUserRank extends CActiveRecord
{
	/**
	 * @return string the associated database table name
	 */
	public function tableName()
	{
		return 'stat_user_rank';
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
			array('user_id, total_clean_volume, inner_pm25_index, outer_pm25_index, city, date, add_time', 'required'),
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
	 * (non-PHPdoc)
	 * @see CActiveRecord::scopes()
	 */
	public function scopes()
	{
		return array(
			'recently' => array(
				'order' => 'id DESC',
				'limit' => 1
			)
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
			'total_clean_volume' => 'Total Clean Volume',
			'volumn_ratio' => 'Volumn Ratio',
			'inner_pm25_index' => 'Inner Pm25 Index',
			'pm25_ratio' => 'Pm25 Ratio',
			'outer_pm25_index' => 'Outer Pm25 Index',
			'date' => 'Date',
			'add_time' => 'Add Time',
		);
	}

	/**
	 * Returns the static model of the specified AR class.
	 * Please note that you should have this exact method in all your CActiveRecord descendants!
	 * @param string $className active record class name.
	 * @return UserRank the static model class
	 */
	public static function model($className=__CLASS__)
	{
		return parent::model($className);
	}
	
	/**
	 * 获取用户昨天全家净化排行
	 * @param unknown $user_id
	 */
	public function detail($user_id)
	{
		$data = array(
			'first' => 0,  // 是否是第一天分享1是 0不是
			'city' => '北京',
			'total_clean_volume' => 200,
			'volumn_ratio' => '22%',
			'inner_pm25_index' => 23,
			'pm25_ratio' => '25%',
			'outer_pm25_index' => 200,
		);
		$result = $this->recently()->findByAttributes(array('user_id'=>$user_id));
		if (!empty($result))
			$data = array(
				'first' => 0,
				'city' => $result->city,
				'total_clean_volume' => $result->total_clean_volume,
				'volumn_ratio' => $result->volumn_ratio,
				'inner_pm25_index' => $result->inner_pm25_index,
				'pm25_ratio' => $result->pm25_ratio,
				'outer_pm25_index' => $result->outer_pm25_index,
			);
		return $data;
	}
}
