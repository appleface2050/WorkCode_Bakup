<?php

/**
 * This is the model class for table "user_delivery_address".
 *
 * The followings are the available columns in table 'user_delivery_address':
 * @property string $id
 * @property string $user_id
 * @property string $name
 * @property string $mobile
 * @property string $provinceId
 * @property integer $cityId
 * @property string $address
 * @property integer $default
 * @property string $provinceName
 * @property string $cityName
 */
class UserAddress extends CActiveRecord
{
	/**
	 * @return string the associated database table name
	 */
	public function tableName()
	{
		return 'user_delivery_address';
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
			array('user_id, add_time', 'required'),
			array('user_id, name, mobile, provinceId, cityId, address, provinceName, cityName, add_time', 'safe'),
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
			'name' => 'Name',
			'mobile' => 'Mobile',
			'provinceId' => 'Province',
			'cityId' => 'City',
			'address' => 'Address',
			'default' => 'Default',
			'provinceName' => 'Province Name',
			'cityName' => 'City Name',
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
		$criteria->compare('user_id',$this->user_id,true);
		$criteria->compare('name',$this->name,true);
		$criteria->compare('mobile',$this->mobile,true);
		$criteria->compare('provinceId',$this->provinceId,true);
		$criteria->compare('cityId',$this->cityId);
		$criteria->compare('address',$this->address,true);
		$criteria->compare('default',$this->default);
		$criteria->compare('provinceName',$this->provinceName,true);
		$criteria->compare('cityName',$this->cityName,true);

		return new CActiveDataProvider($this, array(
			'criteria'=>$criteria,
		));
	}

	/**
	 * Returns the static model of the specified AR class.
	 * Please note that you should have this exact method in all your CActiveRecord descendants!
	 * @param string $className active record class name.
	 * @return UserAddress the static model class
	 */
	public static function model($className=__CLASS__)
	{
		return parent::model($className);
	}
	
	
	/**
	 * 获取格式化后的信息
	 * @param unknown $user_id
	 */
	public function getFormatInfo($user_id)
	{
		$model = $this->findByAttributes(array('user_id'=>$user_id));
		$data = array('name'=>'','mobile'=>'','provinceId'=>-1,'provinceName'=>'','cityId'=>-1,'cityName'=>'','address'=>'');
		if (!empty($model))
		{
			$data = array(
				'name' => !empty($model->name) ? $model->name : '',
				'mobile' => !empty($model->mobile) ? $model->mobile : '',
				'provinceId' => !empty($model->provinceId) ? intval($model->provinceId) : -1,
				'provinceName' => !empty($model->provinceName) ? $model->provinceName : '',
				'cityId' => !empty($model->cityId) ? intval($model->cityId) : -1,
				'cityName' => !empty($model->cityName) ? $model->cityName : '',
				'address' => !empty($model->address) ? $model->address : ''
			); 
		}
		return $data;
	}
}
