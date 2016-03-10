<?php

/**
 * This is the model class for table "weixin_click".
 *
 * The followings are the available columns in table 'weixin_click':
 * @property string $id
 * @property integer $task_id
 * @property string $cleaner_id
 * @property integer $wx_user_id
 * @property string $remark
 * @property integer $create_time
 */
class WeixinClick extends CActiveRecord
{
	/**
	 * @return string the associated database table name
	 */
	public function tableName()
	{
		return 'weixin_click';
	}

	/**
	 * @return array validation rules for model attributes.
	 */
	public function rules()
	{
		// NOTE: you should only define rules for those attributes that
		// will receive user inputs.
		return array(
			array('task_id, cleaner_id, wx_user_id, create_time', 'required'),
			array('task_id, wx_user_id, create_time', 'numerical', 'integerOnly'=>true),
			array('cleaner_id', 'length', 'max'=>100),
			array('remark', 'length', 'max'=>300),
			// The following rule is used by search().
			// @todo Please remove those attributes that should not be searched.
			array('id, task_id, cleaner_id, wx_user_id, remark, create_time', 'safe', 'on'=>'search'),
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
			'wx_user' => array( self::BELONGS_TO,'UserWeixin','wx_user_id')
		);
	}

	/**
	 * @return array customized attribute labels (name=>label)
	 */
	public function attributeLabels()
	{
		return array(
			'id' => 'ID',
			'task_id' => 'Task',
			'cleaner_id' => 'Cleaner',
			'wx_user_id' => 'Wx User',
			'remark' => 'Remark',
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
		$criteria->compare('task_id',$this->task_id);
		$criteria->compare('cleaner_id',$this->cleaner_id,true);
		$criteria->compare('wx_user_id',$this->wx_user_id);
		$criteria->compare('remark',$this->remark,true);
		$criteria->compare('create_time',$this->create_time);

		return new CActiveDataProvider($this, array(
			'criteria'=>$criteria,
		));
	}

	/**
	 * Returns the static model of the specified AR class.
	 * Please note that you should have this exact method in all your CActiveRecord descendants!
	 * @param string $className active record class name.
	 * @return WeixinClick the static model class
	 */
	public static function model($className=__CLASS__)
	{
		return parent::model($className);
	}
}
