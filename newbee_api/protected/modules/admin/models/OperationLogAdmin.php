<?php

/**
 * This is the model class for table "user_operation_log".
 *
 * The followings are the available columns in table 'user_operation_log':
 * @property string $id
 * @property string $user_id
 * @property string $object_id
 * @property string $operation
 * @property string $after_value
 * @property string $add_time
 */
class OperationLogAdmin extends OperationLog
{
	/**
	 * @return array validation rules for model attributes.
	 */
	public function rules()
	{
		// NOTE: you should only define rules for those attributes that
		// will receive user inputs.
		return array(
			array('user_id, object_id, operation, after_value, add_time', 'required'),
			array('user_id, add_time', 'length', 'max'=>10),
			array('object_id', 'length', 'max'=>30),
			array('operation', 'length', 'max'=>50),
			array('after_value', 'length', 'max'=>1000),
			// The following rule is used by search().
			// @todo Please remove those attributes that should not be searched.
			array('id, user_id, object_id, operation, after_value, add_time', 'safe', 'on'=>'search'),
		);
	}

	/**
	 * @return array customized attribute labels (name=>label)
	 */
	public function attributeLabels()
	{
		return array(
			'id' => 'ID',
			'user_id' => '操作人',
			'object_id' => '净化器ID',
			'operation' => '操作',
			'after_value' => '操作结果',
			'add_time' => '操作时间',
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
		$criteria->compare('object_id',$this->object_id,true);
		$criteria->compare('operation',$this->operation,true);
		$criteria->compare('after_value',$this->after_value,true);
		$criteria->compare('add_time',$this->add_time,true);

        $criteria->order = ' id DESC';
		return new CActiveDataProvider($this, array(
			'criteria'=>$criteria,
		));
	}

	/**
	 * Returns the static model of the specified AR class.
	 * Please note that you should have this exact method in all your CActiveRecord descendants!
	 * @param string $className active record class name.
	 * @return OperationLogAdmin the static model class
	 */
	public static function model($className=__CLASS__)
	{
		return parent::model($className);
	}
}
