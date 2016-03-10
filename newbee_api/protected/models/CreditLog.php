<?php

/**
 * This is the model class for table "credit_log".
 *
 * The followings are the available columns in table 'credit_log':
 * @property string $id
 * @property integer $action_id
 * @property integer $type_id
 * @property integer $credit
 * @property string $credit_info
 * @property string $create_date
 * @property integer $create_time
 */
class CreditLog extends CActiveRecord
{
	const ACTION_LOGIN = 1;  //登录
	const ACTION_SHARE = 2;  //分享
	const ACTION_OPEN_CLEANER_IN_SMOG = 3; //雾霾天开机
	const ACTION_EXCHANGE = 4;     //兑换商品

	const TYPE_PLUS = 1;   //加积分
	const TYPE_MINUS = 2;  //减积分
	/**
	 * @return string the associated database table name
	 */
	public function tableName()
	{
		return 'credit_log';
	}

	/**
	 * @return array validation rules for model attributes.
	 */
	public function rules()
	{
		// NOTE: you should only define rules for those attributes that
		// will receive user inputs.
		return array(
			array('user_id,action_id, type_id, credit, credit_info, create_date, create_time', 'required'),
			array('user_id,action_id, type_id, credit, create_time', 'numerical', 'integerOnly'=>true),
			array('credit_info', 'length', 'max'=>100),
			// The following rule is used by search().
			// @todo Please remove those attributes that should not be searched.
			array('id, user_id, action_id, type_id, credit, credit_info, create_date, create_time', 'safe', 'on'=>'search'),
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
			'user_id' => 'user_id',
			'action_id' => 'Action',
			'type_id' => 'Type',
			'credit' => 'Credit',
			'credit_info' => 'Credit Info',
			'create_date' => 'Create Date',
			'create_time' => 'Create Time',
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
		$criteria->compare('action_id',$this->action_id);
		$criteria->compare('type_id',$this->type_id);
		$criteria->compare('credit',$this->credit);
		$criteria->compare('credit_info',$this->credit_info,true);
		$criteria->compare('create_date',$this->create_date,true);
		$criteria->compare('create_time',$this->create_time);

		return new CActiveDataProvider($this, array(
			'criteria'=>$criteria,
		));
	}

	/**
	 * Returns the static model of the specified AR class.
	 * Please note that you should have this exact method in all your CActiveRecord descendants!
	 * @param string $className active record class name.
	 * @return CreditLog the static model class
	 */
	public static function model($className=__CLASS__)
	{
		return parent::model($className);
	}

	/**
	 * 记录日志
	 * @param $action_id
	 * @param $credit
	 * @param $credit_info
	 */
	public function saveLog($user_id,$action_id,$credit,$credit_info)
	{
		$model = new self;
		$model->attributes = array(
			'user_id' => $user_id,
			'action_id' => $action_id,
			'credit' => $credit,
			'credit_info' => $credit_info,
			'create_date' => date('Y-m-d'),
			'create_time' => time()
		);
		$model->type_id = $action_id == self::ACTION_EXCHANGE ? self::TYPE_MINUS : self::TYPE_PLUS;
		$model->save();
	}

	/**
	 * 今日任务
	 * @param $user_id
	 * @param $action_id
	 * @param $credit
	 * @param $credit_info
	 */
	public function doTodayTask($user_id,$action_id)
	{
		if(!$user_id || !$action_id)
			return;
		$task = self::getTaskType($action_id);
		if(!$task)
			return;
		//判断今天是否给过积分
		$condition = "user_id = {$user_id} AND action_id = {$action_id} AND create_date ='".date('Y-m-d')."'";
		$result = $this->find($condition);
		if(!$result)
		{
			$this->saveLog($user_id,$action_id,$task['credit'],$task['name']);
		}
	}

	/**
	 * 获取任务类型
	 * @param null $id
	 */
	public static function getTaskType($action_id = null)
	{
		$arr = array(
			self::ACTION_LOGIN => array('name' =>'登录签到','credit' => 10),
			self::ACTION_SHARE => array('name' =>'分享','credit' => 20),
			self::ACTION_OPEN_CLEANER_IN_SMOG => array('name' =>'雾霾天开机','credit' => 10),
		);
		if($action_id === null)
			return $arr;
		return isset($arr[$action_id]) ? $arr[$action_id] : array();
	}
}
