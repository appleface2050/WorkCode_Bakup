<?php

/**
 * This is the model class for table "cleaner".
 *
 * The followings are the available columns in table 'cleaner':
 * @property string $qrcode
 * @property string $cleaner_id
 * @property integer $release_date
 * @property string $version
 * @property string $add_time
 * @property integer $type
 */
class Cleaner extends CActiveRecord
{
	/**
	 * @return string the associated database table name
	 */
	public function tableName()
	{
		return 'cleaner';
	}

	/**
	 * @return array validation rules for model attributes.
	 */
	public function rules()
	{
		// NOTE: you should only define rules for those attributes that
		// will receive user inputs.
		return array(
			array('qrcode, release_date, version, add_time, type', 'required'),
			// The following rule is used by search().
			// @todo Please remove those attributes that should not be searched.
			array('qrcode, cleaner_id, release_date, version, add_time, type', 'safe'),
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
			'qrcode' => 'Qrcode',
			'cleaner_id' => 'Cleaner',
			'release_date' => 'Release Date',
			'version' => 'Version',
			'add_time' => 'Add Time',
			'type' => 'Type',
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

		$criteria->compare('qrcode',$this->qrcode,true);
		$criteria->compare('cleaner_id',$this->cleaner_id,true);
		$criteria->compare('release_date',$this->release_date);
		$criteria->compare('version',$this->version,true);
		$criteria->compare('add_time',$this->add_time,true);
		$criteria->compare('type',$this->type);

		return new CActiveDataProvider($this, array(
			'criteria'=>$criteria,
		));
	}

	/**
	 * Returns the static model of the specified AR class.
	 * Please note that you should have this exact method in all your CActiveRecord descendants!
	 * @param string $className active record class name.
	 * @return Cleaner the static model class
	 */
	public static function model($className=__CLASS__)
	{
		return parent::model($className);
	}

	/**
	 * 根据二维码获取相应的wifi模块
	 * @param $qrcode
	 * @return array|bool
	 */
	public function getWifiTypeByQrcode($qrcode)
	{
		$code = substr($qrcode,0,2);
		$type = $desc = '';
		switch($code)
		{
			case '01':     //高达老版 守护老版本
			case '03':
				$type = 1;
				$desc = 'RAK415,高达老版,守护';
				break;
			case '06':     //伊娃 高达新版 守护新版
			case '07':
			case '08':
				$type = 2;
			    $desc = 'HFLPB-100,伊娃 高达新版';
				break;
		}
		if(!$type)
			return false;
		return array('wifi_module_type' => $type,'desc' => $desc);
	}
}
