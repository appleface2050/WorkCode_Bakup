<?php


/**
 * This is the model class for table "admin".
 *
 * The followings are the available columns in table 'admin':
 * @property integer $id
 * @property string $username
 * @property string $password
 * @property integer $superuser
 * @property integer $status
 * @property string $create_at
 */
class Admin extends CActiveRecord
{
    const SUPER_USER_YES = 1;
    const SUPER_USER_NO = 0;

	/**
	 * @return string the associated database table name
	 */
	public function tableName()
	{
		return 'admin';
	}

	/**
	 * @return array validation rules for model attributes.
	 */
	public function rules()
	{
		// NOTE: you should only define rules for those attributes that
		// will receive user inputs.
		return array(
			array('create_at,username,password', 'required'),
			array('superuser, status', 'numerical', 'integerOnly'=>true),
			array('username', 'length', 'max'=>20),
			array('password', 'length', 'max'=>128),
            array('username','unique'),
			// The following rule is used by search().
			// @todo Please remove those attributes that should not be searched.
			array('id, username, password, superuser, status, create_at', 'safe', 'on'=>'search'),
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
			'username' => '账号',
			'password' => '密码',
			'superuser' => '超级管理员？',
			'status' => '状态',
			'create_at' => '创建时间',
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

		$criteria->compare('id',$this->id);
		$criteria->compare('username',$this->username,true);
		$criteria->compare('password',$this->password,true);
		$criteria->compare('superuser',$this->superuser);
		$criteria->compare('status',$this->status);
		$criteria->compare('create_at',$this->create_at,true);

		return new CActiveDataProvider($this, array(
			'criteria'=>$criteria,
		));
	}

	/**
	 * Returns the static model of the specified AR class.
	 * Please note that you should have this exact method in all your CActiveRecord descendants!
	 * @param string $className active record class name.
	 * @return Admin the static model class
	 */
	public static function model($className=__CLASS__)
	{
		return parent::model($className);
	}
	
	
	
	/**
	 * 密码加密
	 * @param unknown $string
	 */
	public function encrypting($password)
	{
		$key = 'Air%&$NEW&%RBEER';
		return md5($key . $password);
	}

    /*
     */
    public function beforeSave()
    {
        if ($this->getIsNewRecord())
        {
            $this->password = $this->encrypting($this->password);
            $this->create_at = time();
        }else
		{
			$this->password && $this->password = $this->encrypting($this->password);
		}
        return parent::beforeSave();
    }
}
