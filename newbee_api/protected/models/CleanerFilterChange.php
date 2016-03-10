<?php

/**
 * This is the model class for table "cleaner_filter_change".
 *
 * The followings are the available columns in table 'cleaner_filter_change':
 * @property string $id
 * @property string $cleaner_id
 * @property integer $filter_id
 * @property integer $reset_time
 */
class CleanerFilterChange extends CActiveRecord
{
	/**
	 * @return string the associated database table name
	 */
	public function tableName()
	{
		return 'cleaner_filter_change';
	}

	/**
	 * @return array validation rules for model attributes.
	 */
	public function rules()
	{
		// NOTE: you should only define rules for those attributes that
		// will receive user inputs.
		return array(
			array('cleaner_id, filter_id, reset_time', 'required'),
			array('filter_id, reset_time', 'numerical', 'integerOnly'=>true),
			array('cleaner_id', 'length', 'max'=>50),
			// The following rule is used by search().
			// @todo Please remove those attributes that should not be searched.
			array('id, cleaner_id, filter_id, reset_time', 'safe', 'on'=>'search'),
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
			'cleaner_id' => 'Cleaner',
			'filter_id' => 'Filter',
			'reset_time' => 'Reset Time',
		);
	}

	/**
	 * Retrieves a list of models based on the current search/filter conditions.
	 *
	 * Typical usecase:
	 * - Initialize the model fields with values from filter form.
	 * - Execute this method to get CActiveDataProvider instance which will filter
	 * models according to data in model fields.
	 * - Pass data provider to CGridView, CListView or any similar widget.
	 *
	 * @return CActiveDataProvider the data provider that can return the models
	 * based on the search/filter conditions.
	 */
	public function search()
	{
		// @todo Please modify the following code to remove attributes that should not be searched.

		$criteria=new CDbCriteria;

		$criteria->compare('id',$this->id,true);
		$criteria->compare('cleaner_id',$this->cleaner_id,true);
		$criteria->compare('filter_id',$this->filter_id);
		$criteria->compare('reset_time',$this->reset_time);

		return new CActiveDataProvider($this, array(
			'criteria'=>$criteria,
		));
	}

	/**
	 * Returns the static model of the specified AR class.
	 * Please note that you should have this exact method in all your CActiveRecord descendants!
	 * @param string $className active record class name.
	 * @return CleanerFilterChange the static model class
	 */
	public static function model($className=__CLASS__)
	{
		return parent::model($className);
	}
	/**
	 * 重置净化器滤芯,记录日志
	 * @param $cleaner_id
	 * @param $filter_id
	 */
	public function reset($cleaner_id,$filter_id)
	{
		$condition = "cleaner_id = '{$cleaner_id}' AND filter_id = ".$filter_id;
		$model = $this->find($condition);
		if(!$model)
		{
			$model = new self;
			$model->cleaner_id = $cleaner_id;
			$model->filter_id = $filter_id;
		}
		$model->reset_time = time();
		$model->save();
	}

	/**
	 * 获取开机天数
	 * @param $cleaner_id
	 * @return array
	 */
	public function getOpenedDays($cleaner)
	{
		$cleaner_id = $cleaner->id;
		$data = array();
		//获取总的开机天数
		$opened_days = OnlineCleanerHistory::model()->count("cleaner_id = '{$cleaner_id}' AND date_char >= '2015-11-01'");//开机时间从2015-11-01开始算起
		//获取重置后的开机天数
		$result = $this->findAllByAttributes(array('cleaner_id' => $cleaner_id));
		if($result)
		{
			foreach($result as $val)
			{
				//$data[$val->filter_id] = ceil( (time() - $val->reset_time)/86400 );
				$reset_day = date('Y-m-d',$val->reset_time);
				$data[$val->filter_id] = OnlineCleanerHistory::model()->count("cleaner_id = '{$cleaner_id}' AND date_char >= '{$reset_day}'");
			}
		}
		$filter_surplus_life = json_decode($cleaner->filter_surplus_life,true);
		foreach($filter_surplus_life as $filter_id=>$life)
		{
			if(!isset($data[$filter_id]))
				$data[$filter_id] = $opened_days;
		}
		return $data;
	}

	/**
	 * 获取滤芯信息
	 * @param $cleaner
	 * @return array
	 */
	public function getFilterData($cleaner,$type_id)
	{
		if(!$type_id)
			return array();
		$surplus = CleanerStatus::countSurplusLife($cleaner->filter_surplus_life,$cleaner->type,$cleaner->version);
		if(!$surplus)
			return array();
		//获取开机天数
		if($type_id == CleanerFreeExchange::TYPE_OLD_CLEANER)
			$open_days_after_reset = CleanerFilterChange::model()->getOpenedDays($cleaner);
		$filterData = array();
		//==========
		$exchangeCondition = $this->getExchangeCondition($cleaner->type);
		if(!$exchangeCondition)
			return array();
		$cleaner_id = $cleaner->id;
		foreach($surplus as $s)
		{
			$s['can_exchange'] = false;//如果要做测试,可以临时修改成ture
			$s['opened_days'] = 0;
			$s['share_total'] = 0;
			$s['filter_img_url'] = Yii::app()->request->hostInfo .'/change_filter/img/'.$cleaner->type.'_'.$s['id'].'.jpg?v=1';

			$reset_time = $this->getFilterResetTime($cleaner->id,$s['id']);
			//获取分享次数
			$s['share_total'] = WeixinShare::model()->count("cleaner_id = '{$cleaner_id}' AND filter_id = {$s['id']} AND add_time > {$reset_time}");
			$filter_condition = $exchangeCondition[$s['id']];

			if(in_array($type_id,array(CleanerFreeExchange::TYPE_NEW_CLEANER,CleanerFreeExchange::TYPE_FREE_FIVE_YEARS)))
			{
				//判断是否超过兑换次数
				$exchange_counts = FilterExchange::model()->getExchangeCounts($cleaner_id,$s['id']);
				if($exchange_counts < $filter_condition['exchange_count_limit'])
				{
					//获取最近的一次兑换，判断本次兑换是否重置了滤芯
					$latestExchange = FilterExchange::model()->getLatestExchange($cleaner_id,$s['id']);
					if(!$latestExchange  || $latestExchange->create_time < $reset_time)
					{
						if($s['life_value'] < $filter_condition['surplus_life'] && $s['share_total'] >= $filter_condition['share_total'])
							$s['can_exchange'] = true;
					}
				}
			}else
			{
				if(isset( $open_days_after_reset[$s['id']] ))
				{
					$s['opened_days'] = $open_days_after_reset[$s['id']];
					//判断滤芯重置后是否兑换过
					$exchange = FilterExchange::model()->checkExchanged($cleaner_id,$s['id'],$reset_time);
					if(!$exchange)
					{
						if($s['opened_days'] >= $filter_condition['opened_days'] && $s['share_total'] >= $filter_condition['share_total'])
							$s['can_exchange'] = true;
					}
				}
			}
			unset($s['life'],$s['surplus_life']);
			$s['filter_condition'] = $filter_condition;
			$filterData[$s['id']] = $s;
		}
		return $filterData;
	}

	/**
	 * 兑换条件
	 * @param $type
	 * @return array
	 */
	public function getExchangeCondition($type)
	{
		$arr = array(
			//高达卫士
			CleanerStatus::TYPE_BIG_WITHOUT => array(
				1 => array('surplus_life' => 20,'exchange_count_limit' => 1,'share_total' => 15,'opened_days'=> 540),
				2 => array('surplus_life' => 20,'exchange_count_limit' => 1,'share_total' => 15,'opened_days'=> 360),
				3 => array('surplus_life' => 20,'exchange_count_limit' => 3,'share_total' => 15,'opened_days'=> 180),
			),
			//圆形机器：守护系列
			CleanerStatus::TYPE_CIRCULAR => array(
				1 => array('surplus_life' => 20,'exchange_count_limit' => 1,'share_total' => 15,'opened_days'=> 180),
			),
			//伊娃系列
			CleanerStatus::TYPE_YIWA => array(
				1 => array('surplus_life' => 20,'exchange_count_limit' => 1,'share_total' => 15,'opened_days'=> 180),
			),
		);
		return isset($arr[$type]) ? $arr[$type] : array();
	}

	/**
	 * 获取滤芯重置时间
	 * @param $cleaner_id
	 * @param $filter_id
	 * @return int|mixed|null
	 */
	public function getFilterResetTime($cleaner_id,$filter_id)
	{
		$result = $this->findByAttributes(array('cleaner_id' => $cleaner_id,'filter_id' => $filter_id));
		return $result ? $result->reset_time : 0;
	}
}
