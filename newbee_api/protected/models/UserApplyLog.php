<?php

/**
 * This is the model class for table "user_apply_log".
 *
 * The followings are the available columns in table 'user_apply_log':
 * @property string $id
 * @property string $user_id
 * @property string $cleaner_id
 * @property string $create_time
 * @property integer $status
 * @property string $note
 * @property string $verify_time
 */
class UserApplyLog extends CActiveRecord
{
	
	/**
	 * 申请状态 1申请中 2通过 3拒绝
	 * Enter description here ...
	 * @var unknown_type
	 */
	const APPLY_STATUS_APPLY  = 1;
	const APPLY_STATUS_PASS   = 2;
	const APPLY_STATUS_REFUSE = 3;
	
	
	/**
	 * @return string the associated database table name
	 */
	public function tableName()
	{
		return 'user_apply_log';
	}

	/**
	 * @return array validation rules for model attributes.
	 */
	public function rules()
	{
		// NOTE: you should only define rules for those attributes that
		// will receive user inputs.
		return array(
			array('apply_uid, cleaner_id, apply_tid,add_time', 'required'),
			array('note', 'length', 'max'=>50),
			// The following rule is used by search().
			// @todo Please remove those attributes that should not be searched.
			array('id, apply_uid, cleaner_id, add_time, status, note, verify_time, verify_uid, apply_tid', 'safe'),
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
			'cleaner' => array(self::BELONGS_TO, 'CleanerStatus', 'cleaner_id'),
			'user'    => array(self::BELONGS_TO, 'User', 'apply_uid')
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
			'add_time' => 'Create Time',
			'status' => 'Status',
			'note' => 'Note',
			'verify_time' => 'Verify Time',
			'verify_uid' => 'verify_uid'
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
		$criteria->compare('apply_uid',$this->user_id,true);
		$criteria->compare('cleaner_id',$this->cleaner_id,true);
		$criteria->compare('add_time',$this->create_time,true);
		$criteria->compare('status',$this->status);
		$criteria->compare('note',$this->note,true);
		$criteria->compare('verify_time',$this->verify_time,true);

		return new CActiveDataProvider($this, array(
			'criteria'=>$criteria,
		));
	}

	/**
	 * Returns the static model of the specified AR class.
	 * Please note that you should have this exact method in all your CActiveRecord descendants!
	 * @param string $className active record class name.
	 * @return UserApplyLog the static model class
	 */
	public static function model($className=__CLASS__)
	{
		return parent::model($className);
	}
}
