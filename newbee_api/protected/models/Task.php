<?php

/**
 * This is the model class for table "task".
 *
 * The followings are the available columns in table 'task':
 * @property string $id
 * @property integer $user_id
 * @property string $task_name
 * @property string $cleaner_id
 * @property integer $filter_id
 * @property string $filter_name
 * @property integer $click_number
 * @property string $encrypt_code
 * @property integer $finish
 * @property integer $create_time
 * @property integer $finish_time
 */
class Task extends CActiveRecord
{
	//开机100天才允许兑换滤芯
	const CHANGE_FILTER_NEED_OPEN_DAYS = 100;
	//滤芯寿命 <20% 才允许兑换滤芯
	const CHANGE_FILTER_NEED_SURPLUS_LIFE = 20;

	const TASK_FINISH_YES = 1;
	const TASK_FINISH_NO = 0;

	const SUPPORT_LIMIT = 15; //微信支持限制人数

	/**
	 * @return string the associated database table name
	 */
	public function tableName()
	{
		return 'task';
	}

	/**
	 * @return array validation rules for model attributes.
	 */
	public function rules()
	{
		// NOTE: you should only define rules for those attributes that
		// will receive user inputs.
		return array(
			array('user_id, task_name, cleaner_id, filter_id, encrypt_code, create_time', 'required'),
			array('user_id, filter_id, click_number, finish, create_time, finish_time', 'numerical', 'integerOnly'=>true),
			array('task_name, cleaner_id, filter_name, encrypt_code', 'length', 'max'=>100),
			// The following rule is used by search().
			// @todo Please remove those attributes that should not be searched.
			array('id, user_id, task_name, cleaner_id, filter_id, filter_name, click_number, encrypt_code, finish, create_time, finish_time', 'safe', 'on'=>'search'),
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
			'task_name' => 'Task Name',
			'cleaner_id' => 'Cleaner',
			'filter_id' => 'Filter',
			'filter_name' => 'Filter Name',
			'click_number' => 'Click Number',
			'encrypt_code' => 'Encrypt Code',
			'finish' => 'Finish',
			'create_time' => 'Create Time',
			'finish_time' => 'Finish Time',
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
		$criteria->compare('task_name',$this->task_name,true);
		$criteria->compare('cleaner_id',$this->cleaner_id,true);
		$criteria->compare('filter_id',$this->filter_id);
		$criteria->compare('filter_name',$this->filter_name,true);
		$criteria->compare('click_number',$this->click_number);
		$criteria->compare('encrypt_code',$this->encrypt_code,true);
		$criteria->compare('finish',$this->finish);
		$criteria->compare('create_time',$this->create_time);
		$criteria->compare('finish_time',$this->finish_time);

		return new CActiveDataProvider($this, array(
			'criteria'=>$criteria,
		));
	}

	/**
	 * Returns the static model of the specified AR class.
	 * Please note that you should have this exact method in all your CActiveRecord descendants!
	 * @param string $className active record class name.
	 * @return Task the static model class
	 */
	public static function model($className=__CLASS__)
	{
		return parent::model($className);
	}
}
