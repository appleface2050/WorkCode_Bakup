<?php

/**
 * This is the model class for table "goods".
 *
 * The followings are the available columns in table 'goods':
 * @property string $id
 * @property string $name
 * @property string $cover
 * @property integer $cost_credit
 * @property string $market_price
 * @property integer $quantity
 * @property integer $exchange_index
 * @property integer $unlock_days
 * @property integer $status
 * @property integer $create_time
 * @property integer $update_time
 */
class Goods extends CActiveRecord
{

	const STATUS_OFFLINE = 0; //下架
	const STATUS_ONLINE = 1;  //上架
	/**
	 * @return string the associated database table name
	 */
	public function tableName()
	{
		return 'goods';
	}

	/**
	 * @return array validation rules for model attributes.
	 */
	public function rules()
	{
		// NOTE: you should only define rules for those attributes that
		// will receive user inputs.
		return array(
			array('create_time', 'default', 'value'=>time()),
			array('update_time', 'default', 'value'=>time()),
			array('name, cost_credit, market_price, exchange_index, create_time, update_time', 'required'),
			array('cost_credit, quantity, exchange_index, unlock_days, status, create_time, update_time, rank', 'numerical', 'integerOnly'=>true),
			array('name, cover', 'length', 'max'=>100),
			array('market_price', 'length', 'max'=>10),
			// The following rule is used by search().
			// @todo Please remove those attributes that should not be searched.
			array('id, name, cover, cost_credit, market_price, quantity, exchange_index, unlock_days, status, create_time, update_time', 'safe', 'on'=>'search'),
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
			'name' => '名称',
			'cover' => '图片',
			'cost_credit' => '需要积分',
			'market_price' => '市场价',
			'quantity' => '数量',
			'exchange_index' => '兑换指数',
			'unlock_days' => '解锁天数',
			'status' => '状态',
			'create_time' => '创建时间',
			'update_time' => '更新时间',
            'rank' => '显示权重'
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
		$criteria->compare('name',$this->name,true);
		$criteria->compare('cover',$this->cover,true);
		$criteria->compare('cost_credit',$this->cost_credit);
		$criteria->compare('market_price',$this->market_price,true);
		$criteria->compare('quantity',$this->quantity);
		$criteria->compare('exchange_index',$this->exchange_index);
		$criteria->compare('unlock_days',$this->unlock_days);
		$criteria->compare('status',$this->status);
		$criteria->compare('create_time',$this->create_time);
		$criteria->compare('update_time',$this->update_time);

		return new CActiveDataProvider($this, array(
			'criteria'=>$criteria,
		));
	}

	/**
	 * Returns the static model of the specified AR class.
	 * Please note that you should have this exact method in all your CActiveRecord descendants!
	 * @param string $className active record class name.
	 * @return Goods the static model class
	 */
	public static function model($className=__CLASS__)
	{
		return parent::model($className);
	}

	/**
	 * 获取状态
	 * @param null $id
	 */
	public static function getStatus($id = null)
	{
		$arr = array(
			self::STATUS_OFFLINE => '下架',
			self::STATUS_ONLINE => '上架'
		);
		if ($id === null)
			return $arr;
		return isset($arr[$id]) ? $arr[$id] : '';
	}

	public function beforeSave()
	{
		if(!$this->isNewRecord)
		{
			$this->update_time = time();
		}
		return parent::beforeSave();
	}

	public function afterValidate()
	{
        if($this->hasErrors())
            return;
		$file = CUploadedFile::getInstance($this, 'cover');
		if (!empty($file))
		{
			$attachHelper = new AttachHelper($file);
			$this->cover = $attachHelper->save();
		}
		return parent::afterValidate();
	}

}
