<?php

/**
 * 所有具有PM25的城市的最近一次PM25值
 * This is the model class for table "pm25_outside_index".
 *
 * The followings are the available columns in table 'pm25_outside_index':
 * @property string $city
 * @property string $name
 * @property integer $pm25_index
 * @property string $update_time
 */
class AqiIndex extends CActiveRecord
{
	/**
	 * @return string the associated database table name
	 */
	public function tableName()
	{
		return 'aqi_outside_index';
	}

	/**
	 * @return array validation rules for model attributes.
	 */
	public function rules()
	{
		// NOTE: you should only define rules for those attributes that
		// will receive user inputs.
		return array(
			array('city, name, aqi, update_time, time_point', 'required'),
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
			'city' => '城市',
			'name' => 'Name',
			'aqi' => 'PM25',
			'update_time' => '更新时间',
			'time_point' => '检测时间'
		);
	}

	/**
	 * Returns the static model of the specified AR class.
	 * Please note that you should have this exact method in all your CActiveRecord descendants!
	 * @param string $className active record class name.
	 * @return PmIndex the static model class
	 */
	public static function model($className=__CLASS__)
	{
		return parent::model($className);
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
	
		$criteria->compare('city',$this->city,true);
	
		return new CActiveDataProvider($this, array(
				'criteria'=>$criteria,
				'sort' => array(
					'attributes' => array('city')
				)
		));
	}
	
}
