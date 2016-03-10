<?php

/**
 * This is the model class for table "cleaner_free_exchange".
 *
 * The followings are the available columns in table 'cleaner_free_exchange':
 * @property string $id
 * @property string $qrcode
 * @property integer $create_time
 */
class CleanerFreeExchange extends CActiveRecord
{

	const TYPE_NEW_CLEANER = 1;  //新机器  终身免费
	const TYPE_OLD_CLEANER = 2;  //老机器  终身免费
	const TYPE_FREE_FIVE_YEARS = 3;//5年免费兑换滤芯的机器
	/**
	 * @return string the associated database table name
	 */
	public function tableName()
	{
		return 'cleaner_free_exchange';
	}

	/**
	 * @return array validation rules for model attributes.
	 */
	public function rules()
	{
		// NOTE: you should only define rules for those attributes that
		// will receive user inputs.
		return array(
			array('create_time','default','value' => time()),
			array('qrcode, create_time', 'required'),
			array('type_id, create_time', 'numerical', 'integerOnly'=>true),
			array('qrcode', 'length', 'max'=>50),
			array('remark', 'length', 'max'=>200),
			// The following rule is used by search().
			// @todo Please remove those attributes that should not be searched.
			array('id, qrcode, create_time', 'safe', 'on'=>'search'),

			array('qrcode','unique'),
			array('qrcode', 'checkExists'),
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
			'qrcode' => 'SN号',
			'remark' => '备注',
			'create_time' => '添加时间',
			'type_id' => '分类',
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
		$criteria->compare('qrcode',$this->qrcode,true);
		$criteria->compare('create_time',$this->create_time);
		$criteria->compare('type_id',$this->type_id);

		$criteria->order = 'id DESC';
		return new CActiveDataProvider($this, array(
			'criteria'=>$criteria,
		));
	}

	/**
	 * Returns the static model of the specified AR class.
	 * Please note that you should have this exact method in all your CActiveRecord descendants!
	 * @param string $className active record class name.
	 * @return CleanerFreeExchange the static model class
	 */
	public static function model($className=__CLASS__)
	{
		return parent::model($className);
	}

	/**
	 * 判断是否可以参与活动
	 * @param $cleaner_id
	 * @return array
	 */
	public function checkCanJoinActivity($cleaner_id,$cleaner = null)
	{
		if(!$cleaner)
			$cleaner = CleanerStatus::model()->findByPk($cleaner_id);
		if(!$cleaner || !$cleaner->qrcode)
			return array(0, '无效的净化器');
		$free_exchange = CleanerFreeExchange::model()->findByAttributes(array('qrcode' => $cleaner->qrcode));
		if($free_exchange)
		{
			//判断是否在规定年限之内
			if($free_exchange->type_id == self::TYPE_FREE_FIVE_YEARS)
			{
				if(time() - $cleaner->first_use_time > 24 * 3600 * 365 * 5)
				{
					return array(0, '5年免费更换滤芯期限已到,该净化器不能参与该活动');
				}
			}
			return array($free_exchange->type_id, '可以参与活动');
		}
		return array(0, '该净化器不能参与该活动');
	}

	/**
	 * 获取类型
	 * @param null $status
	 * @return array|string
	 */
	public static function getType($status = null)
	{
		$arr = array(
			self::TYPE_NEW_CLEANER => '新机器',
			self::TYPE_OLD_CLEANER=> '老机器',
			self::TYPE_FREE_FIVE_YEARS => '5年免费兑换'
		);
		if($status === null)
			return $arr;
		return isset($arr[$status]) ? $arr[$status] : '';
	}

	public function checkExists($attribute,$params)
	{
		$cleaner = Cleaner::model()->findByAttributes(array('qrcode'=>$this->qrcode));
		if(!$cleaner)
			$this->addError($attribute, '无效的SN号码!');
	}
}
