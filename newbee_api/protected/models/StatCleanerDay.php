<?php

/**
 * This is the model class for table "stat_cleaner_day".
 *
 * The followings are the available columns in table 'stat_cleaner_day':
 * @property string $id
 * @property string $cleaner_id
 * @property integer $pm25_index
 * @property string $clean_volumn
 * @property string $date
 * @property string $add_time
 */
class StatCleanerDay extends CActiveRecord
{
	/**
	 * @return string the associated database table name
	 */
	public function tableName()
	{
		return 'stat_cleaner_day';
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
			array('cleaner_id, pm25_index, clean_volumn, date, add_time', 'required'),
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
			'cleaner_id' => 'Cleaner',
			'pm25_index' => 'Pm25 Index',
			'clean_volumn' => 'Clean Volumn',
			'date' => 'Date',
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
		$criteria->compare('cleaner_id',$this->cleaner_id,true);
		$criteria->compare('pm25_index',$this->pm25_index);
		$criteria->compare('clean_volumn',$this->clean_volumn,true);
		$criteria->compare('date',$this->date,true);
		$criteria->compare('add_time',$this->add_time,true);

		return new CActiveDataProvider($this, array(
			'criteria'=>$criteria,
		));
	}

	/**
	 * Returns the static model of the specified AR class.
	 * Please note that you should have this exact method in all your CActiveRecord descendants!
	 * @param string $className active record class name.
	 * @return StatCleanerDay the static model class
	 */
	public static function model($className=__CLASS__)
	{
		return parent::model($className);
	}
}
