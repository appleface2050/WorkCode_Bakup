<?php

/**
 * This is the model class for table "user_feedback".
 *
 * The followings are the available columns in table 'user_feedback':
 * @property string $id
 * @property string $user_id
 * @property integer $type
 * @property string $detail
 * @property string $add_time
 * @property integer $status
 * @property integer $update_time
 */
class Feedback extends CActiveRecord
{
	
	/**
	 * 是否解决,1解决了 0 未解决
	 * @var unknown
	 */
	const STATUS_UNRESOLVED = 0;
	const STATUS_RESOLVEED  = 1;
	
	
	/**
	 * 故障类型
	 * @var unknown
	 */
	public static $types = array(
		1 => '净化器不工作',
		2 => '净化器不显示数值'
	);
	
	
	/**
	 * @return string the associated database table name
	 */
	public function tableName()
	{
		return 'user_feedback';
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
			array('user_id, type, detail, add_time, status', 'required'),
			array('detail', 'length', 'max'=>255),
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
			'user' => array(self::BELONGS_TO, 'User', 'user_id')
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
			'type' => 'Type',
			'detail' => 'Detail',
			'add_time' => 'Add Time',
			'status' => 'Status',
			'update_time' => 'Update Time',
		);
	}

	/**
	 * Returns the static model of the specified AR class.
	 * Please note that you should have this exact method in all your CActiveRecord descendants!
	 * @param string $className active record class name.
	 * @return Feedback the static model class
	 */
	public static function model($className=__CLASS__)
	{
		return parent::model($className);
	}
}
