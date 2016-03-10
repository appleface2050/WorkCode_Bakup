<?php
// 用户操作日志表
/**
 * This is the model class for table "user_operation_log".
 *
 * The followings are the available columns in table 'user_operation_log':
 * @property string $id
 * @property string $user_id
 * @property string $operation
 * @property string $create_time
 * @property string $after_value
 */
class OperationLog extends CActiveRecord
{
	
	/**
	 * 操作类型
	 * @var unknown
	 */
	const OPERATION_CHANGE_CHILDLOCK = 'CHANGE_CHILDLOCK';
	const OPERATION_CHANGE_LEVEL     = 'CHANGE_LEVEL';
	const OPERATION_CHANGE_AUTOMATIC = 'CHANGE_AUTOMATIC';
	const OPERATION_CLEANER_CLOSE	 = 'CLEANER_CLOSE';
	const OPERATION_CLEANER_OPEN	 = 'CLEANER_OPEN';
	const OPERATION_CHANGE_TIMESET   = 'CHANGE_TIMESET';
	const OPERATION_CHANGE_SILENCE   = 'CHANGE_SILENCE';
	const OPERATION_FILTER_RESET     = 'FILTER_RESET';
	const OPERATION_CLEANER_UPGRADE  = 'CLEANER_UPGRADE';
	const OPERATION_CLEANER_LED_ONOFF= 'CLEANER_LED_ONOFF';
	
	
	
	/**
	 * @return string the associated database table name
	 */
	public function tableName()
	{
		return 'user_operation_log';
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
			array('user_id, object_id, operation, add_time', 'required'),
			array('after_value', 'safe')
			// The following rule is used by search().
			// @todo Please remove those attributes that should not be searched.
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
		$criteria->compare('add_time',$this->add_time,true);
		$criteria->compare('after_value',$this->after_value,true);

        $criteria->order = ' id DESC';

		return new CActiveDataProvider($this, array(
			'criteria'=>$criteria,
		));
	}

	/**
	 * Returns the static model of the specified AR class.
	 * Please note that you should have this exact method in all your CActiveRecord descendants!
	 * @param string $className active record class name.
	 * @return OperationLog the static model class
	 */
	public static function model($className=__CLASS__)
	{
		return parent::model($className);
	}

    /**
     * 获取操作名称
     * @param $action
     * @return string
     */
    public static function getOperationName($action)
    {
        $opName = '';
        switch($action)
        {
            case self::OPERATION_CHANGE_CHILDLOCK:
                $opName = '童锁';
                break;

            case self::OPERATION_CHANGE_LEVEL:
                $opName = '更换档位';
                break;

            case self::OPERATION_CHANGE_AUTOMATIC:
                $opName = '自动';
                break;

            case self::OPERATION_CLEANER_CLOSE:
                $opName = '关闭';
                break;

            case self::OPERATION_CLEANER_OPEN:
                $opName = '开启';
                break;

            case self::OPERATION_CHANGE_TIMESET:
                $opName = '定时';
                break;

            case self::OPERATION_CHANGE_SILENCE:
                $opName = '静音';
                break;

            case self::OPERATION_FILTER_RESET:
                $opName = '重启';
                break;

            case self::OPERATION_CLEANER_UPGRADE:
                $opName = '升级';
                break;
        }
        return $opName;
    }
}
