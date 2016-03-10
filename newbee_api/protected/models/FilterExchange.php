<?php

/**
 * This is the model class for table "filter_exchange".
 *
 * The followings are the available columns in table 'filter_exchange':
 * @property string $id
 * @property integer $user_id
 * @property string $cleaner_id
 * @property integer $filter_id
 * @property string $filter_name
 * @property integer $create_time
 * @property string $receiver_name
 * @property string $receiver_mobile
 * @property string $receiver_address
 * @property string $remark
 * @property string $shipping_info
 * @property integer $status
 */
class FilterExchange extends CActiveRecord
{

    const STATUS_CREATE = 1;   //未处理
    const STATUS_SHIPPING = 2;  //发货
    const STATUS_EXCEPTION = 9;  //异常
	/**
	 * @return string the associated database table name
	 */
	public function tableName()
	{
		return 'filter_exchange';
	}

	/**
	 * @return array validation rules for model attributes.
	 */
	public function rules()
	{
		// NOTE: you should only define rules for those attributes that
		// will receive user inputs.
		return array(
			array('user_id, cleaner_id, filter_id, create_time, receiver_name, receiver_mobile, receiver_address', 'required'),
			array('user_id, filter_id, create_time, status', 'numerical', 'integerOnly'=>true),
			array('cleaner_id, filter_name, receiver_address', 'length', 'max'=>100),
			array('receiver_name', 'length', 'max'=>30),
			array('receiver_mobile', 'length', 'max'=>11),
			array('remark, shipping_info', 'length', 'max'=>300),
			// The following rule is used by search().
			// @todo Please remove those attributes that should not be searched.
			array('id, user_id, cleaner_id, filter_id, filter_name, create_time, receiver_name, receiver_mobile, receiver_address, remark, shipping_info,status', 'safe', 'on'=>'search'),
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
			'cleaner' => array( self::BELONGS_TO,'CleanerStatus','cleaner_id'),
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
			'cleaner_id' => 'Cleaner',
			'filter_id' => '滤芯编号',
			'filter_name' => '滤芯名称',
			'create_time' => '兑换时间',
			'receiver_name' => '姓名',
			'receiver_mobile' => '电话',
			'receiver_address' => '地址',
			'remark' => '备注',
			'shipping_info' => '发货信息',
            'status' => '状态'
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
		$criteria->compare('user_id',$this->user_id);
		$criteria->compare('cleaner_id',$this->cleaner_id,true);
		$criteria->compare('filter_id',$this->filter_id);
		$criteria->compare('filter_name',$this->filter_name,true);
		$criteria->compare('create_time',$this->create_time);
		$criteria->compare('receiver_name',$this->receiver_name,true);
		$criteria->compare('receiver_mobile',$this->receiver_mobile,true);
		$criteria->compare('receiver_address',$this->receiver_address,true);
        //$criteria->compare('status',$this->status);

        $criteria->with = 'cleaner';
        $criteria->order = 'create_time DESC';
		return new CActiveDataProvider($this, array(
			'criteria'=>$criteria,
		));
	}

	/**
	 * Returns the static model of the specified AR class.
	 * Please note that you should have this exact method in all your CActiveRecord descendants!
	 * @param string $className active record class name.
	 * @return FilterExchange the static model class
	 */
	public static function model($className=__CLASS__)
	{
		return parent::model($className);
	}

	/**
	 * 判断是否兑换过
	 * @param $cleaner_id
	 * @param $filter_id
	 * @param null $reset_time
	 * @return CActiveRecord
	 */
	public function checkExchanged($cleaner_id,$filter_id,$reset_time = null)
	{
		if($reset_time === null)
			$reset_time = CleanerFilterChange::model()->getFilterResetTime($cleaner_id,$filter_id);
		$exchange = FilterExchange::model()->find(sprintf("cleaner_id = '%s' AND filter_id = %s AND create_time > {$reset_time}",$cleaner_id,$filter_id));
		return $exchange;
	}

	/**
	 * 获取每年兑换滤芯次数
	 * @param $cleaner_id
	 * @param $filter_id
	 */
	public function getExchangeCounts($cleaner_id,$filter_id)
	{
		$start_time = strtotime(date('Y-01-01 00:00:00'));
		$end_time = strtotime(date('Y-12-31 23:59:59'));
		return FilterExchange::model()->count(sprintf("cleaner_id = '%s' AND filter_id = %s AND create_time >= {$start_time} and create_time <= {$end_time} ",$cleaner_id,$filter_id));
	}

    /**
     * 获取兑换状态
     * @param null $status
     * @return array|string
     */
    public static function getStatus($status = null)
    {
        $arr = array(
            self::STATUS_CREATE => '未处理',
            self::STATUS_SHIPPING => '已发货',
            self::STATUS_EXCEPTION => '异常'
        );
        if($status === null)
            return $arr;
        return isset($arr[$status]) ? $arr[$status] : '';
    }

	/**
	 * 获取每年兑换滤芯次数
	 * @param $cleaner_id
	 * @param $filter_id
	 */
	public function getLatestExchange($cleaner_id,$filter_id)
	{
		$exchange=FilterExchange::model()->find(array(
			'select'=>'create_time',
			'condition'=>sprintf("cleaner_id = '%s' AND filter_id = %s ",$cleaner_id,$filter_id),
			'order' => ' create_time DESC'
		));
		return $exchange;
	}
}
