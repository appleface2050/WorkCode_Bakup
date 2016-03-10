<?php

/**
 * This is the model class for table "weixin_user".
 *
 * The followings are the available columns in table 'weixin_user':
 * @property string $id
 * @property string $weixin_openid
 * @property string $username
 * @property string $headimgurl
 * @property integer $sex
 * @property integer $add_time
 */
class UserWeixin extends CActiveRecord
{
	/**
	 * @return string the associated database table name
	 */
	public function tableName()
	{
		return 'weixin_user';
	}

	/**
	 * @return array validation rules for model attributes.
	 */
	public function rules()
	{
		// NOTE: you should only define rules for those attributes that
		// will receive user inputs.
		return array(
			array('username, add_time', 'required'),
			array('sex, add_time', 'numerical', 'integerOnly'=>true),
			array('weixin_openid', 'length', 'max'=>50),
			array('username', 'length', 'max'=>60),
			array('headimgurl', 'length', 'max'=>500),
			// The following rule is used by search().
			// @todo Please remove those attributes that should not be searched.
			array('id, weixin_openid, username, headimgurl, sex, add_time', 'safe', 'on'=>'search'),
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
			'weixin_openid' => 'Weixin Openid',
			'username' => 'Username',
			'headimgurl' => 'Headimgurl',
			'sex' => 'Sex',
			'add_time' => 'Add Time',
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
		$criteria->compare('weixin_openid',$this->weixin_openid,true);
		$criteria->compare('username',$this->username,true);
		$criteria->compare('headimgurl',$this->headimgurl,true);
		$criteria->compare('sex',$this->sex);
		$criteria->compare('add_time',$this->add_time);

		return new CActiveDataProvider($this, array(
			'criteria'=>$criteria,
		));
	}

	/**
	 * Returns the static model of the specified AR class.
	 * Please note that you should have this exact method in all your CActiveRecord descendants!
	 * @param string $className active record class name.
	 * @return UserWeixin the static model class
	 */
	public static function model($className=__CLASS__)
	{
		return parent::model($className);
	}
}
