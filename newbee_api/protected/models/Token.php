<?php

/**
 * This is the model class for table "user_token".
 *
 * The followings are the available columns in table 'user_token':
 * @property string $user_id
 * @property string $token
 * @property string $add_time
 */
class Token extends CActiveRecord
{
	
	/**
	 * token有效时常  10分钟
	 * Enter description here ...
	 * @var unknown_type
	 */
	const  LIFETIME = 600;
	
	/**
	 * @return string the associated database table name
	 */
	public function tableName()
	{
		return 'user_token';
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
			array('user_id, token, platform, add_time', 'required'),
		);
	}

	/**
	 * @return array customized attribute labels (name=>label)
	 */
	public function attributeLabels()
	{
		return array(
			'user_id' => 'User',
			'token' => 'Token',
			'add_time' => 'Add Time',
		);
	}
	
	/**
	 * (non-PHPdoc)
	 * @see CActiveRecord::relations()
	 */
	public function relations()
	{
		return array(
			'user' => array(self::BELONGS_TO, 'User', 'user_id')
		);
	}
	/**
	 * Returns the static model of the specified AR class.
	 * Please note that you should have this exact method in all your CActiveRecord descendants!
	 * @param string $className active record class name.
	 * @return Token the static model class
	 */
	public static function model($className=__CLASS__)
	{
		return parent::model($className);
	}
	
	/**
	 * 根据用户id获取token
	 */
	public static function generateToken($user_id)
	{
		return md5($user_id.time().rand(10, 100));
	}
	
	
	/**
	 * 注销token
	 * @param string $token
	 */
	public function destoryToken($token)
	{
		$this->deleteAllByAttributes(array('token'=>$token));
	}
	
	/**
	 * 重置token
	 * @param $origin 原token
	 */
	public function reset($origin)
	{
		$model = Token::model()->with('user')->findByAttributes(array('token'=>$origin));
		if(empty($model) || empty($model->user))
			throw new CHttpException(403, 'token不存在');
		$token = self::generateToken($model->user_id);
		Token::model()->updateByPk($model->user_id, array('token' => $token, 'add_time' => time()));
		return array('token' => $token, 'lifetime' => self::LIFETIME);
	}
}
