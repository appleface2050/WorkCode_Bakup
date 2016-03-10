<?php

/**
 * This is the model class for table "cleaner_video_log".
 *
 * The followings are the available columns in table 'cleaner_video_log':
 * @property string $cleaner_id
 * @property string $video_id
 * @property integer $status
 * @property integer $duration
 * @property string $image
 * @property string $add_time
 */
class CleanerVideo extends CActiveRecord
{
	/**
	 * @return string the associated database table name
	 */
	public function tableName()
	{
		return 'cleaner_video_log';
	}

	/**
	 * @return array validation rules for model attributes.
	 */
	public function rules()
	{
		// NOTE: you should only define rules for those attributes that
		// will receive user inputs.
		return array(
			array('qrcode, video_id, add_time', 'required'),
			//array('qrcode', 'length', 'max'=>30),
			//array('add_time', 'length', 'max'=>10),
			// The following rule is used by search().
			// @todo Please remove those attributes that should not be searched.
			array('qrcode, video_id, status, duration, image, add_time', 'safe'),
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
			'cleaner_id' => 'Cleaner',
			'video_id' => 'Video',
			'status' => 'Status',
			'duration' => 'Duration',
			'image' => 'Image',
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

		$criteria->compare('cleaner_id',$this->cleaner_id,true);
		$criteria->compare('video_id',$this->video_id,true);
		$criteria->compare('status',$this->status);
		$criteria->compare('duration',$this->duration);
		$criteria->compare('image',$this->image,true);
		$criteria->compare('add_time',$this->add_time,true);

		return new CActiveDataProvider($this, array(
			'criteria'=>$criteria,
		));
	}

	/**
	 * Returns the static model of the specified AR class.
	 * Please note that you should have this exact method in all your CActiveRecord descendants!
	 * @param string $className active record class name.
	 * @return CleanerVideo the static model class
	 */
	public static function model($className=__CLASS__)
	{
		return parent::model($className);
	}
}
