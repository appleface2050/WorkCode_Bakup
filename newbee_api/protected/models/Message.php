<?php

/**
 * This is the model class for table "system_message".
 *
 * The followings are the available columns in table 'system_message':
 * @property string $id
 * @property string $title
 * @property string $content
 * @property string $img
 * @property string $add_time
 */
class Message extends CActiveRecord
{
	/**
	 * @return string the associated database table name
	 */
	public function tableName()
	{
		return 'system_message';
	}

	/**
	 * @return array validation rules for model attributes.
	 */
	public function rules()
	{
		// NOTE: you should only define rules for those attributes that
		// will receive user inputs.
		return array(
			array('title, content, add_time', 'required'),
			array('title, img', 'length', 'max'=>30),
			array('content', 'length', 'max'=>255),
			array('add_time', 'length', 'max'=>10),
			// The following rule is used by search().
			// @todo Please remove those attributes that should not be searched.
			array('id, title, content, img, add_time', 'safe', 'on'=>'search'),
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
			'title' => 'Title',
			'content' => 'Content',
			'img' => 'Img',
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
		$criteria->compare('title',$this->title,true);
		$criteria->compare('content',$this->content,true);
		$criteria->compare('img',$this->img,true);
		$criteria->compare('add_time',$this->add_time,true);

		return new CActiveDataProvider($this, array(
			'criteria'=>$criteria,
		));
	}

	/**
	 * Returns the static model of the specified AR class.
	 * Please note that you should have this exact method in all your CActiveRecord descendants!
	 * @param string $className active record class name.
	 * @return Message the static model class
	 */
	public static function model($className=__CLASS__)
	{
		return parent::model($className);
	}
	
	
/**
	 * 获取最新的系统消息
	 * @param string $timestamp
	 */
	public function getLatestMessage($timestamp=null)
	{
		$sql = "SELECT * FROM ".$this->tableName();
		if (!empty($timestamp))
			$sql .= " WHERE add_time>{$timestamp}";
		$sql .= " ORDER BY id DESC";
		$result = Yii::app()->db->createCommand($sql)->queryAll();
		$data = array();
		if (!empty($result))
		{
			foreach ($result as $value)
			{
				$data[] = array(
					'id' => $value['id'],
					'title' => $value['title'],
					'content' => $value['content'],
					'add_time' => intval($value['add_time']),
					//'img' => $this->getAbsoluteAvatar($value['img'])
				);
			}
		}
		return $data;
	}
}
