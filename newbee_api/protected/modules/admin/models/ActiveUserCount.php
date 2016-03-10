<?php

/**
 * This is the model class for table "active_user_count".
 *
 * The followings are the available columns in table 'active_user_count':
 * @property string $id
 * @property integer $date
 * @property string $total
 * @property string $add_time
 * @property string $date_char
 */
class ActiveUserCount extends CActiveRecord
{
	/**
	 * @return string the associated database table name
	 */
	public function tableName()
	{
		return 'active_user_count';
	}

	/**
	 * @return array validation rules for model attributes.
	 */
	public function rules()
	{
		// NOTE: you should only define rules for those attributes that
		// will receive user inputs.
		return array(
			array('date, total, add_time, date_char', 'required'),
			array('date', 'numerical', 'integerOnly'=>true),
			array('total, add_time, date_char', 'length', 'max'=>10),
			// The following rule is used by search().
			// @todo Please remove those attributes that should not be searched.
			array('id, date, total, add_time, date_char', 'safe'),
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
			'date' => '日期',
			'total' => '总数',
			'add_time' => '统计时间',
			'date_char' => '日期',
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

		$criteria->compare('date',$this->date);
		$criteria->order = 'date DESC';

		return new CActiveDataProvider($this, array(
			'criteria'=>$criteria
		));
	}

	/**
	 * Returns the static model of the specified AR class.
	 * Please note that you should have this exact method in all your CActiveRecord descendants!
	 * @param string $className active record class name.
	 * @return ActiveUserCount the static model class
	 */
	public static function model($className=__CLASS__)
	{
		return parent::model($className);
	}
}
